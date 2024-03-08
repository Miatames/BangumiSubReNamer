using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using BangumiSubReNamer.Models;
using BangumiSubReNamer.Services;
using CommunityToolkit.Mvvm.Messaging;
using Wpf.Ui.Controls;

namespace BangumiSubReNamer.ViewModels.Pages
{
    public partial class MediaRenamerViewModel : ObservableObject, INavigationAware,
        IRecipient<DataWindowSize>, IRecipient<DataEpisodesInfoList>
    {
        public MediaRenamerViewModel()
        {
            WeakReferenceMessenger.Default.Register<DataWindowSize>(this);
            WeakReferenceMessenger.Default.Register<DataEpisodesInfoList>(this);

            Console.WriteLine("init MediaRenamerViewModel");
        }

        [ObservableProperty] private int height = 580;
        [ObservableProperty] private Visibility isProcess = Visibility.Hidden;
        [ObservableProperty] private bool isNewFileListChecked = false;
        [ObservableProperty] private Visibility isSourceFileList = Visibility.Visible;
        [ObservableProperty] private Visibility isNewFileList = Visibility.Hidden;
        [ObservableProperty] private ObservableCollection<DataFilePath> sourceFileList = new();
        [ObservableProperty] private ObservableCollection<DataEpisodesInfo> episodesInfoList = new();
        [ObservableProperty] private ObservableCollection<DataFilePath> newFileList = new();

        [ObservableProperty] private bool isAddNfoFile = true;
        [ObservableProperty] private int currentSearchMode = 0;
        [ObservableProperty] private int currentFileOperateMode = 0;

        private string sourceFileEndsRegex = "";
        private string subFileEndsRegex = "";

        public void Receive(DataWindowSize message)
        {
            Height = message.Height - 70;
        }

        public void Receive(DataEpisodesInfoList message)
        {
            // EpisodesInfoList.Clear();
            foreach (var dataEpisodesInfo in message.Infos)
            {
                Console.WriteLine(dataEpisodesInfo.ShowText);
                EpisodesInfoList.Add(dataEpisodesInfo);
            }

            CreateNewFileList();
        }

        partial void OnIsNewFileListCheckedChanged(bool value)
        {
            if (value)
            {
                IsSourceFileList = Visibility.Hidden;
                IsNewFileList = Visibility.Visible;
            }
            else
            {
                IsSourceFileList = Visibility.Visible;
                IsNewFileList = Visibility.Hidden;
            }
        }

        partial void OnCurrentSearchModeChanged(int value)
        {
            CreateNewFileList();
        }

        [RelayCommand]
        private async Task OnSearch()
        {
            if (SourceFileList.Count <= 0) return;

            var searchStrList = new List<string>();

            foreach (var dataFilePath in SourceFileList)
            {
                //使用AnitomySharp获取剧集名称，在括号内的剧集名称无法获取
                var result = AnitomySharp.AnitomySharp.Parse(dataFilePath.FileName);

                foreach (var element in result)
                {
                    switch (element.Category.ToString())
                    {
                        case "ElementAnimeTitle":
                            if (element.Value == "")
                            {
                                WeakReferenceMessenger.Default.Send<DataSnackbarMessage>(
                                    new DataSnackbarMessage("获取名称失败", dataFilePath.FileName, ControlAppearance.Caution));
                                break;
                            }
                            else
                            {
                                searchStrList.AddUnique(element.Value);
                                break;
                            }
                        case "ElementEpisodeNumber":
                            break;
                    }
                }
            }

            if (searchStrList.Count <= 0) return;

            IsProcess = Visibility.Visible;
            try
            {
                foreach (var searchStr in searchStrList)
                {
                    //使用BangumiApi搜素剧集信息，英文名可能搜不出
                    var results = await BangumiApiConfig.Instance.BangumiApi_Search(searchStr);
                    if (results.Count <= 0) continue;

                    //默认获取第一条
                    var jsonSubjects = JsonSerializer.Deserialize<BgmApiJson_Search>(results[0]);
                    if (jsonSubjects == null) continue;
                    if (jsonSubjects.list.Count <= 0)
                    {
                        WeakReferenceMessenger.Default.Send<DataSnackbarMessage>(new DataSnackbarMessage("搜索无结果",
                            $"搜索：{searchStr}",
                            ControlAppearance.Caution));
                        continue;
                    }

                    var subjectId = jsonSubjects.list[0].id;

                    var episodes = await BangumiApiConfig.Instance.BangumiApi_Episodes(subjectId.ToString());
                    var jsonEpisodes = JsonSerializer.Deserialize<BgmApiJson_EpisodesInfo>(episodes);
                    if (jsonEpisodes == null) continue;

                    foreach (var listItem in jsonEpisodes.data)
                    {
                        EpisodesInfoList.Add(new DataEpisodesInfo(listItem.id, listItem.name, listItem.name_cn,
                            jsonSubjects.list[0].name, jsonSubjects.list[0].name_cn, listItem.ep, listItem.sort,
                            listItem.subject_id, 0, DateTime.Parse(jsonSubjects.list[0].air_date).Year.ToString()));
                    }

                    //剧集模式搜索SP
                    if (CurrentSearchMode != 0) continue;

                    var episodesSP = await BangumiApiConfig.Instance.BangumiApi_EpisodesSp(subjectId.ToString());
                    var jsonEpisodesSp = JsonSerializer.Deserialize<BgmApiJson_EpisodesInfo>(episodesSP);
                    if (jsonEpisodesSp == null) continue;

                    foreach (var listItem in jsonEpisodesSp.data)
                    {
                        EpisodesInfoList.Add(new DataEpisodesInfo(listItem.id, listItem.name, listItem.name_cn,
                            jsonSubjects.list[0].name, jsonSubjects.list[0].name_cn, listItem.ep, listItem.sort,
                            listItem.subject_id, 1, DateTime.Parse(jsonSubjects.list[0].air_date).Year.ToString()));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            IsProcess = Visibility.Hidden;

            CreateNewFileList();
        }

        private void CreateNewFileList()
        {
            var targetFolder = GlobalConfig.Instance.OutFilePath;
            NewFileList.Clear();

            for (int i = 0; i < Math.Min(SourceFileList.Count, EpisodesInfoList.Count); i++)
            {
                var sourcePath = SourceFileList[i].FilePath;
                var sourceName = SourceFileList[i].FileName;

                var padleft = (Math.Min(SourceFileList.Count, EpisodesInfoList.Count)).ToString().Length;

                var newPath = "";
                var newName = "";
                //根据剧集和电影文件规则生成路径和文件名
                switch (CurrentSearchMode)
                {
                    case 0:
                        if (EpisodesInfoList[i].Type == 0)
                        {
                            newName =
                                $"{EpisodesInfoList[i].SubjectNameCn} - E{EpisodesInfoList[i].Sort.ToString().PadLeft(padleft,'0')} - {EpisodesInfoList[i].NameCn} - {sourceName}";
                            newPath = targetFolder.Replace("{RootPath}", Path.GetPathRoot(sourcePath)) +
                                      EpisodesInfoList[i].SubjectNameCn + $" ({EpisodesInfoList[i].Year})" + @"\Season 1\";
                        }
                        else
                        {
                            newName =
                                $"{EpisodesInfoList[i].SubjectNameCn} - S0E{EpisodesInfoList[i].Sort.ToString().PadLeft(padleft,'0')} - {EpisodesInfoList[i].NameCn} - {sourceName}";
                            newPath = targetFolder.Replace("{RootPath}", Path.GetPathRoot(sourcePath)) +
                                      EpisodesInfoList[i].SubjectNameCn + $" ({EpisodesInfoList[i].Year})" + @"\SP\";
                        }

                        break;
                    case 1:
                        newName = sourceName;
                        newPath = targetFolder.Replace("{RootPath}", Path.GetPathRoot(sourcePath)) +
                                  EpisodesInfoList[i].SubjectNameCn + $" ({EpisodesInfoList[i].Year})" + @"\";
                        break;
                }

                //排除非法的路径和文件名字符
                NewFileList.Add(new DataFilePath(newPath.RemoveInvalidPathNameChar() + newName.RemoveInvalidFileNameChar(),
                    newName.RemoveInvalidFileNameChar()));
            }
        }

        [RelayCommand]
        private void OnClearSourceFileList()
        {
            SourceFileList.Clear();
        }

        [RelayCommand]
        private void OnClearEpisodesInfoList()
        {
            EpisodesInfoList.Clear();
        }


        [RelayCommand]
        private void OnClearNewFileList()
        {
            NewFileList.Clear();
        }

        [RelayCommand]
        private void OnClearAll()
        {
            SourceFileList.Clear();
            EpisodesInfoList.Clear();
            NewFileList.Clear();
        }

        [RelayCommand]
        private void OnCreateNewFileList()
        {
            CreateNewFileList();
        }

        [RelayCommand]
        private async Task OnRunFileOperate()
        {
            Console.WriteLine("类型" + CurrentFileOperateMode);

            IsProcess = Visibility.Visible;
            switch (CurrentFileOperateMode)
            {
                case 0:
                    await RunHardLinkFiles();
                    break;
                case 1:
                    await RunCopyFiles();
                    break;
                case 2:
                    await RunRenameFiles();
                    break;
                default:
                    break;
            }

            IsProcess = Visibility.Hidden;
        }


        public void OnNavigatedTo()
        {
            // IsProcess = Visibility.Hidden;
            sourceFileEndsRegex = GlobalConfig.Instance.ReNamerConfig.AddSourceFileExtensionRegex;
            subFileEndsRegex = GlobalConfig.Instance.ReNamerConfig.AddSubFileExtensionRegex;
            IsNewFileListChecked = false;
            Height = GlobalConfig.Instance.Height - 70;
        }

        public void OnNavigatedFrom() { }

        public void AddDropFile(List<string> filePathArrayOrder)
        {
            var dictList = new List<string>();

            foreach (var filePath in filePathArrayOrder)
            {
                var addDataFilePath = new DataFilePath(filePath: filePath, fileName: Path.GetFileName(filePath));
                if (Regex.IsMatch(Path.GetExtension(filePath), sourceFileEndsRegex))
                {
                    SourceFileList.AddUnique(addDataFilePath);
                    dictList.AddUnique(Path.GetFileName(filePath));
                }
                else if (Regex.IsMatch(Path.GetExtension(filePath), subFileEndsRegex))
                {
                    WeakReferenceMessenger.Default.Send<DataFilePath>(addDataFilePath);
                }
                else
                {
                    Console.WriteLine($"unknow add file: {filePath}");
                    WeakReferenceMessenger.Default.Send<DataSnackbarMessage>(
                        new DataSnackbarMessage("未知文件类型" + Path.GetExtension(filePath), filePath, ControlAppearance.Caution));
                }
            }

            GetAniInfo(dictList);
        }

        private void GetAniInfo(List<string> strList)
        {
            string title = "";

            foreach (var str in strList)
            {
                try
                {
                    var result = AnitomySharp.AnitomySharp.Parse(str);
                    foreach (var element in result)
                    {
                        if (element.Category.ToString() == "ElementAnimeTitle")
                        {
                            Console.WriteLine(element.Value);
                            title = element.Value;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }

            WeakReferenceMessenger.Default.Send<DataSearchStrMessage>(new DataSearchStrMessage(title));
            
            CreateNewFileList();
        }

        private async Task RunHardLinkFiles()
        {
            var msg = "";
            await Task.Run(() =>
            {
                for (int i = 0; i < Math.Min(Math.Min(SourceFileList.Count, EpisodesInfoList.Count), NewFileList.Count); i++)
                {
                    if (!File.Exists(SourceFileList[i].FilePath)) continue;

                    var targetPath = Path.GetDirectoryName(NewFileList[i].FilePath);
                    if (targetPath == null) continue;

                    if (!Directory.Exists(targetPath))
                    {
                        Directory.CreateDirectory(targetPath);
                        Console.WriteLine("create: " + targetPath);
                    }

                    if (IsAddNfoFile && !File.Exists(Path.GetDirectoryName(targetPath) + @"\tvshow.nfo"))
                    {
                        var subjectsInfo = new NfoInfo_SubjectsRoot
                        {
                            bangumiid = EpisodesInfoList[i].SubjectId.ToString(),
                            title = EpisodesInfoList[i].SubjectNameCn,
                            originaltitle = EpisodesInfoList[i].SubjectName,
                            showtitle = EpisodesInfoList[i].SubjectNameCn,
                            year = EpisodesInfoList[i].Year
                        };
                        if (CurrentSearchMode == 0)
                        {
                            RunCreateNfoFileSubjects(subjectsInfo, Path.GetDirectoryName(targetPath) + @"\tvshow.nfo");
                        }
                        else
                        {
                            RunCreateNfoFileSubjects(subjectsInfo, targetPath + @"\tvshow.nfo");
                        }
                    }

                    try
                    {
                        ExtensionTools.CreateHardLink(NewFileList[i].FilePath, SourceFileList[i].FilePath, IntPtr.Zero);
                        msg += NewFileList[i].FilePath + "\n";
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    if (IsAddNfoFile && CurrentSearchMode == 0)
                    {
                        var episodesInfo = new NfoInfo_EpisodesRoot
                        {
                            bangumiid = EpisodesInfoList[i].Id.ToString(),
                            title = EpisodesInfoList[i].Name,
                            originaltitle = EpisodesInfoList[i].NameCn,
                            showtitle = EpisodesInfoList[i].Name,
                            episode = EpisodesInfoList[i].Sort.ToString(),
                            season = "1"
                        };
                        RunCreateNfoFileEpisodes(episodesInfo,
                            Path.GetDirectoryName(NewFileList[i].FilePath) + @"\" +
                            Path.GetFileNameWithoutExtension(NewFileList[i].FileName) + ".nfo");
                    }
                }
            });
            WeakReferenceMessenger.Default.Send<DataSnackbarMessage>(
                new DataSnackbarMessage("创建硬链接", msg, ControlAppearance.Info));
        }

        private async Task RunCopyFiles()
        {
            var msg = "";
            await Task.Run(() =>
            {
                for (int i = 0; i < Math.Min(Math.Min(SourceFileList.Count, EpisodesInfoList.Count), NewFileList.Count); i++)
                {
                    if (!File.Exists(SourceFileList[i].FilePath)) continue;
                    var targetPath = Path.GetDirectoryName(NewFileList[i].FilePath);
                    if (targetPath == null) continue;
                    if (!Directory.Exists(targetPath))
                    {
                        Directory.CreateDirectory(targetPath);
                        Console.WriteLine("create: " + targetPath);
                    }

                    if (IsAddNfoFile && !File.Exists(Path.GetDirectoryName(targetPath) + @"\tvshow.nfo"))
                    {
                        var subjectsInfo = new NfoInfo_SubjectsRoot
                        {
                            bangumiid = EpisodesInfoList[i].SubjectId.ToString(),
                            title = EpisodesInfoList[i].SubjectNameCn,
                            originaltitle = EpisodesInfoList[i].SubjectName,
                            showtitle = EpisodesInfoList[i].SubjectNameCn,
                            year = EpisodesInfoList[i].Year
                        };
                        if (CurrentSearchMode == 0)
                        {
                            RunCreateNfoFileSubjects(subjectsInfo, Path.GetDirectoryName(targetPath) + @"\tvshow.nfo");
                        }
                        else
                        {
                            RunCreateNfoFileSubjects(subjectsInfo, targetPath + @"\tvshow.nfo");
                        }
                    }

                    try
                    {
                        File.Copy(SourceFileList[i].FilePath, NewFileList[i].FilePath, true);
                        msg += NewFileList[i].FilePath + "\n";
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    if (IsAddNfoFile && CurrentSearchMode == 0)
                    {
                        var episodesInfo = new NfoInfo_EpisodesRoot
                        {
                            bangumiid = EpisodesInfoList[i].Id.ToString(),
                            title = EpisodesInfoList[i].Name,
                            originaltitle = EpisodesInfoList[i].NameCn,
                            showtitle = EpisodesInfoList[i].Name,
                            episode = EpisodesInfoList[i].Sort.ToString(),
                            season = "1"
                        };
                        RunCreateNfoFileEpisodes(episodesInfo,
                            Path.GetDirectoryName(NewFileList[i].FilePath) + @"\" +
                            Path.GetFileNameWithoutExtension(NewFileList[i].FileName) + ".nfo");
                    }
                }
            });
            WeakReferenceMessenger.Default.Send<DataSnackbarMessage>(
                new DataSnackbarMessage("复制", msg, ControlAppearance.Info));
        }

        private async Task RunRenameFiles()
        {
            var msg = "";
            await Task.Run(() =>
            {
                for (int i = 0; i < Math.Min(Math.Min(SourceFileList.Count, EpisodesInfoList.Count), NewFileList.Count); i++)
                {
                    if (!File.Exists(SourceFileList[i].FilePath)) continue;

                    var newPath = Path.GetDirectoryName(SourceFileList[i].FilePath) + @"\" + NewFileList[i].FileName;

                    //重命名时不创建tvshow.nfo

                    try
                    {
                        File.Move(SourceFileList[i].FilePath, newPath);
                        msg += NewFileList[i].FilePath + "\n";
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    if (IsAddNfoFile && CurrentSearchMode == 0)
                    {
                        var episodesInfo = new NfoInfo_EpisodesRoot
                        {
                            bangumiid = EpisodesInfoList[i].Id.ToString(),
                            title = EpisodesInfoList[i].Name,
                            originaltitle = EpisodesInfoList[i].NameCn,
                            showtitle = EpisodesInfoList[i].Name,
                            episode = EpisodesInfoList[i].Sort.ToString(),
                            season = "1"
                        };
                        RunCreateNfoFileEpisodes(episodesInfo,
                            Path.GetDirectoryName(SourceFileList[i].FilePath) + @"\" +
                            Path.GetFileNameWithoutExtension(NewFileList[i].FileName) + ".nfo");
                    }
                }
            });
            WeakReferenceMessenger.Default.Send<DataSnackbarMessage>(
                new DataSnackbarMessage("重命名", msg, ControlAppearance.Info));
        }

        private void RunCreateNfoFileSubjects(NfoInfo_SubjectsRoot subjectsInfo, string filePath)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(NfoInfo_SubjectsRoot));
                using var writer = new XmlTextWriter(filePath, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                var namespaces = new XmlSerializerNamespaces(new[]
                {
                    new XmlQualifiedName(string.Empty, "")
                });
                serializer.Serialize(writer, subjectsInfo, namespaces);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void RunCreateNfoFileEpisodes(NfoInfo_EpisodesRoot episodesInfo, string filePath)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(NfoInfo_EpisodesRoot));
                using var writer = new XmlTextWriter(filePath, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                var namespaces = new XmlSerializerNamespaces(new[]
                {
                    new XmlQualifiedName(string.Empty, "")
                });
                serializer.Serialize(writer, episodesInfo, namespaces);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}