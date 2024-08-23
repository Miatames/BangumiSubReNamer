using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using BangumiSubReNamer.Models;
using BangumiSubReNamer.Services;
using CommunityToolkit.Mvvm.Messaging;
using GongSolutions.Wpf.DragDrop;
using Wpf.Ui.Controls;

namespace BangumiSubReNamer.ViewModels.Pages
{
    public partial class MediaRenamerViewModel : ObservableObject, INavigationAware, IRecipient<DataEpisodesInfoList>, IDropTarget
    {
        public MediaRenamerViewModel()
        {
            WeakReferenceMessenger.Default.Register<DataEpisodesInfoList>(this);

            Console.WriteLine("init MediaRenamerViewModel");
        }

        [ObservableProperty] private Visibility isProcess = Visibility.Hidden;
        [ObservableProperty] private ObservableCollection<DataFilePath> sourceFileList = new();
        [ObservableProperty] private ObservableCollection<DataEpisodesInfo> episodesInfoList = new();
        [ObservableProperty] private ObservableCollection<DataFilePath> newFileList = new();

        [ObservableProperty] private bool isAddNfoFile = true;
        [ObservableProperty] private int currentSearchMode = 0;
        [ObservableProperty] private int currentFileOperateMode = 0;
        [ObservableProperty] private string processText = "";

        private string sourceFileEndsRegex = "";
        private string subFileEndsRegex = "";

        private List<DataFilePath> sourceFileSelected = new();
        private List<DataEpisodesInfo> episodesInfoSelected = new();

        public void Receive(DataEpisodesInfoList message)
        {
            // EpisodesInfoList.Clear();
            foreach (var dataEpisodesInfo in message.Infos)
            {
                Console.WriteLine(dataEpisodesInfo.ShowText);
                EpisodesInfoList.Add(dataEpisodesInfo);
            }

            // CreateNewFileList();
        }

        /*partial void OnCurrentSearchModeChanged(int value)
        {
            CreateNewFileList();
        }*/

        [RelayCommand]
        private void OnNavigateToPreviewWindow()
        {
            CreateNewFileList();

            WeakReferenceMessenger.Default.Send<DataFilePathPreview>(new DataFilePathPreview()
            {
                fileList = NewFileList.ToList()
            });
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
                                searchStrList.AddUnique(element.Value.RemoveInvalidFileNameChar());
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
                    var searchStrOrigin = await BangumiApiConfig.Instance.TmdbApi_Search(searchStr);

                    var results = new List<string>();
                    if (!string.IsNullOrEmpty(searchStrOrigin))
                    {
                        //使用BangumiApi搜素剧集信息，英文名可能搜不出
                        results = await BangumiApiConfig.Instance.BangumiApi_Search(searchStrOrigin);
                    }
                    else
                    {
                        results = await BangumiApiConfig.Instance.BangumiApi_Search(searchStr);
                    }

                    if (results == null || results.Count <= 0)
                    {
                        WeakReferenceMessenger.Default.Send<DataSnackbarMessage>(new DataSnackbarMessage("搜索无结果",
                            $"搜索：{searchStr}",
                            ControlAppearance.Caution));
                        continue;
                    }

                    //默认获取第一条
                    var jsonSubjects = JsonSerializer.Deserialize<BgmApiJson_Search>(results[0]);
                    if (jsonSubjects == null || jsonSubjects.list.Count <= 0)
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

                    var episodesSp = await BangumiApiConfig.Instance.BangumiApi_EpisodesSp(subjectId.ToString());
                    var jsonEpisodesSp = JsonSerializer.Deserialize<BgmApiJson_EpisodesInfo>(episodesSp);
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

            // CreateNewFileList();
        }

        private void CreateNewFileList()
        {
            var targetFolder = GlobalConfig.Instance.OutFilePath;
            NewFileList.Clear();

            //集数补0
            var padleft = Math.Min(SourceFileList.Count, EpisodesInfoList.Count).ToString().Length;

            for (int i = 0; i < Math.Min(SourceFileList.Count, EpisodesInfoList.Count); i++)
            {
                var sourcePath = SourceFileList[i].FilePath;
                var sourceName = SourceFileList[i].FileName;


                var newPath = "";
                var newName = "";
                //根据剧集和电影文件规则生成路径和文件名
                switch (CurrentSearchMode)
                {
                    case 0:
                        if (EpisodesInfoList[i].Type == 0)
                        {
                            newName = BangumiApiConfig.Instance.BangumiNewFileName(EpisodesInfoList[i], sourceName, padleft);
                            newPath = Path.Combine(
                                targetFolder.Replace("{RootPath}", Path.GetPathRoot(sourcePath)),
                                $"{EpisodesInfoList[i].SubjectNameCn} ({EpisodesInfoList[i].Year})",
                                "Season 1");
                        }
                        else
                        {
                            newName = BangumiApiConfig.Instance.BangumiNewFileName(EpisodesInfoList[i], sourceName, padleft);
                            newPath = Path.Combine(
                                targetFolder.Replace("{RootPath}", Path.GetPathRoot(sourcePath)),
                                $"{EpisodesInfoList[i].SubjectNameCn} ({EpisodesInfoList[i].Year})",
                                "SP");
                        }

                        break;
                    case 1:
                        newName = BangumiApiConfig.Instance.MovieNewFileName(EpisodesInfoList[i], sourceName);
                        newPath = Path.Combine(
                            targetFolder.Replace("{RootPath}", Path.GetPathRoot(sourcePath)),
                            $"{EpisodesInfoList[i].SubjectNameCn} ({EpisodesInfoList[i].Year})");
                        break;
                }

                //排除非法的路径和文件名字符
                NewFileList.Add(new DataFilePath(Path.Combine(newPath.RemoveInvalidPathNameChar(), newName.RemoveInvalidFileNameChar()),
                    newName.RemoveInvalidFileNameChar()));
            }
        }

        [RelayCommand]
        private void OnSourceFilesSelectedItemChange(object sender)
        {
            if (sender is not IList list) return;

            sourceFileSelected = list.Cast<DataFilePath>().ToList();
        }

        [RelayCommand]
        private void OnEpisodeInfoSelectedItemChange(object sender)
        {
            if (sender is not IList list) return;

            episodesInfoSelected = list.Cast<DataEpisodesInfo>().ToList();
        }

        [RelayCommand]
        private void OnDelSourceFilesItem()
        {
            for (var i = sourceFileSelected.Count - 1; i >= 0; i--)
            {
                SourceFileList.Remove(sourceFileSelected[i]);
            }
        }

        [RelayCommand]
        private void OnDelEpisodeInfosItem()
        {
            for (var i = episodesInfoSelected.Count - 1; i >= 0; i--)
            {
                EpisodesInfoList.Remove(episodesInfoSelected[i]);
            }
        }

        [RelayCommand]
        private void OnSortSourceFilesItem()
        {
            var list = SourceFileList.ToList();
            list.Sort((pathA, pathB) => ExtensionTools.StrCmpLogicalW(pathA.FilePath, pathB.FilePath));
            SourceFileList = new ObservableCollection<DataFilePath>(list);
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

            CreateNewFileList();

            IsProcess = Visibility.Visible;
            ProcessText = "";
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
            }

            ProcessText = "";
            IsProcess = Visibility.Hidden;
        }


        public void OnNavigatedTo()
        {
            // IsProcess = Visibility.Hidden;
            sourceFileEndsRegex = GlobalConfig.Instance.ReNamerConfig.AddSourceFileExtensionRegex;
            subFileEndsRegex = GlobalConfig.Instance.ReNamerConfig.AddSubFileExtensionRegex;
            // Height = GlobalConfig.Instance.Height - 70;
        }

        public void OnNavigatedFrom()
        {
        }

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
                            title = element.Value.RemoveInvalidFileNameChar();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            WeakReferenceMessenger.Default.Send<DataSearchStrMessage>(new DataSearchStrMessage(title));

            // CreateNewFileList();
        }

        private async Task RunHardLinkFiles()
        {
            var msg = "";
            var count = Math.Min(Math.Min(SourceFileList.Count, EpisodesInfoList.Count), NewFileList.Count);
            await Task.Run(() =>
            {
                for (int i = 0; i < count; i++)
                {
                    if (!File.Exists(SourceFileList[i].FilePath)) continue;

                    var targetPath = Path.GetDirectoryName(NewFileList[i].FilePath);
                    if (targetPath == null) continue;
                    var targetPathDirectory = Path.GetDirectoryName(targetPath);
                    if (targetPathDirectory == null) continue;

                    if (!Directory.Exists(targetPath))
                    {
                        try
                        {
                            Directory.CreateDirectory(targetPath);
                            Console.WriteLine("create: " + targetPath);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            break;
                        }
                    }

                    if (IsAddNfoFile && CurrentSearchMode == 0 &&
                        !File.Exists(Path.Combine(targetPathDirectory, "tvshow.nfo")))
                    {
                        var subjectsInfo = new NfoInfo_SubjectsRootTv
                        {
                            bangumiid = EpisodesInfoList[i].SubjectId.ToString(),
                            title = EpisodesInfoList[i].SubjectNameCn,
                            originaltitle = EpisodesInfoList[i].SubjectName,
                            showtitle = EpisodesInfoList[i].SubjectNameCn,
                            year = EpisodesInfoList[i].Year
                        };
                        if (CurrentSearchMode == 0)
                        {
                            ExtensionTools.RunCreateNfoFile(subjectsInfo, Path.Combine(targetPathDirectory, "tvshow.nfo"));
                        }
                        else
                        {
                            ExtensionTools.RunCreateNfoFile(subjectsInfo, Path.Combine(targetPath, "tvshow.nfo"));
                        }
                    }

                    try
                    {
                        ProcessText = $"{i + 1}/{count}";
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
                            title = EpisodesInfoList[i].NameCn,
                            originaltitle = EpisodesInfoList[i].Name,
                            showtitle = EpisodesInfoList[i].NameCn,
                            episode = EpisodesInfoList[i].Sort.ToString(),
                            season = EpisodesInfoList[i].Type == 0 ? "1" : "0"
                        };
                        ExtensionTools.RunCreateNfoFile(episodesInfo,
                            Path.Combine(Path.GetDirectoryName(NewFileList[i].FilePath) ?? throw new InvalidOperationException(),
                                Path.GetFileNameWithoutExtension(NewFileList[i].FileName) + ".nfo"));
                    }
                    else if (IsAddNfoFile && CurrentSearchMode == 1)
                    {
                        var movieInfo = new NfoInfo_SubjectsRootMovie()
                        {
                            bangumiid = EpisodesInfoList[i].SubjectId.ToString(),
                            title = EpisodesInfoList[i].SubjectNameCn,
                            originaltitle = EpisodesInfoList[i].SubjectName,
                            year = EpisodesInfoList[i].Year
                        };
                        ExtensionTools.RunCreateNfoFile(movieInfo,
                            Path.Combine(Path.GetDirectoryName(NewFileList[i].FilePath) ?? throw new InvalidOperationException(),
                                Path.GetFileNameWithoutExtension(NewFileList[i].FileName) + ".nfo"));
                    }
                }
            });
            WeakReferenceMessenger.Default.Send<DataSnackbarMessage>(
                new DataSnackbarMessage("创建硬链接", msg, ControlAppearance.Info));
        }

        private async Task RunCopyFiles()
        {
            var msg = "";
            var count = Math.Min(Math.Min(SourceFileList.Count, EpisodesInfoList.Count), NewFileList.Count);
            await Task.Run(() =>
            {
                for (int i = 0; i < Math.Min(Math.Min(SourceFileList.Count, EpisodesInfoList.Count), NewFileList.Count); i++)
                {
                    if (!File.Exists(SourceFileList[i].FilePath)) continue;
                    var targetPath = Path.GetDirectoryName(NewFileList[i].FilePath);
                    if (targetPath == null) continue;
                    var targetPathDirectory = Path.GetDirectoryName(targetPath);
                    if (targetPathDirectory == null) continue;

                    if (!Directory.Exists(targetPath))
                    {
                        try
                        {
                            Directory.CreateDirectory(targetPath);
                            Console.WriteLine("create: " + targetPath);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            break;
                        }
                    }

                    if (IsAddNfoFile && !File.Exists(Path.Combine(targetPathDirectory, "tvshow.nfo")))
                    {
                        var subjectsInfo = new NfoInfo_SubjectsRootTv()
                        {
                            bangumiid = EpisodesInfoList[i].SubjectId.ToString(),
                            title = EpisodesInfoList[i].SubjectNameCn,
                            originaltitle = EpisodesInfoList[i].SubjectName,
                            showtitle = EpisodesInfoList[i].SubjectNameCn,
                            year = EpisodesInfoList[i].Year
                        };
                        if (CurrentSearchMode == 0)
                        {
                            ExtensionTools.RunCreateNfoFile(subjectsInfo, Path.Combine(targetPathDirectory, "tvshow.nfo"));
                        }
                        else
                        {
                            ExtensionTools.RunCreateNfoFile(subjectsInfo, Path.Combine(targetPath, "tvshow.nfo"));
                        }
                    }

                    try
                    {
                        ProcessText = $"{i + 1}/{count}";
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
                            title = EpisodesInfoList[i].NameCn,
                            originaltitle = EpisodesInfoList[i].Name,
                            showtitle = EpisodesInfoList[i].NameCn,
                            episode = EpisodesInfoList[i].Sort.ToString(),
                            season = EpisodesInfoList[i].Type == 0 ? "1" : "0"
                        };
                        ExtensionTools.RunCreateNfoFile(episodesInfo,
                            Path.Combine(Path.GetDirectoryName(NewFileList[i].FilePath) ?? throw new InvalidOperationException(),
                                Path.GetFileNameWithoutExtension(NewFileList[i].FileName) + ".nfo"));
                    }
                    else if (IsAddNfoFile && CurrentSearchMode == 1)
                    {
                        var movieInfo = new NfoInfo_SubjectsRootMovie()
                        {
                            bangumiid = EpisodesInfoList[i].SubjectId.ToString(),
                            title = EpisodesInfoList[i].SubjectNameCn,
                            originaltitle = EpisodesInfoList[i].SubjectName,
                            year = EpisodesInfoList[i].Year
                        };
                        ExtensionTools.RunCreateNfoFile(movieInfo,
                            Path.Combine(Path.GetDirectoryName(NewFileList[i].FilePath) ?? throw new InvalidOperationException(),
                                Path.GetFileNameWithoutExtension(NewFileList[i].FileName) + ".nfo"));
                    }
                }
            });
            WeakReferenceMessenger.Default.Send<DataSnackbarMessage>(
                new DataSnackbarMessage("复制", msg, ControlAppearance.Info));
        }

        private async Task RunRenameFiles()
        {
            var msg = "";
            var count = Math.Min(Math.Min(SourceFileList.Count, EpisodesInfoList.Count), NewFileList.Count);
            await Task.Run(() =>
            {
                for (int i = 0; i < Math.Min(Math.Min(SourceFileList.Count, EpisodesInfoList.Count), NewFileList.Count); i++)
                {
                    if (!File.Exists(SourceFileList[i].FilePath)) continue;

                    var newPath = Path.Combine(Path.GetDirectoryName(SourceFileList[i].FilePath) ?? throw new InvalidOperationException(),
                        NewFileList[i].FileName);
                    //重命名时不创建tvshow.nfo

                    try
                    {
                        ProcessText = $"{i + 1}/{count}";
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
                            title = EpisodesInfoList[i].NameCn,
                            originaltitle = EpisodesInfoList[i].Name,
                            showtitle = EpisodesInfoList[i].NameCn,
                            episode = EpisodesInfoList[i].Sort.ToString(),
                            season = EpisodesInfoList[i].Type == 0 ? "1" : "0"
                        };
                        ExtensionTools.RunCreateNfoFile(episodesInfo,
                            Path.Combine(Path.GetDirectoryName(NewFileList[i].FilePath) ?? throw new InvalidOperationException(),
                                Path.GetFileNameWithoutExtension(NewFileList[i].FileName) + ".nfo"));
                    }
                    else if (IsAddNfoFile && CurrentSearchMode == 1)
                    {
                        var movieInfo = new NfoInfo_SubjectsRootMovie()
                        {
                            bangumiid = EpisodesInfoList[i].SubjectId.ToString(),
                            title = EpisodesInfoList[i].SubjectNameCn,
                            originaltitle = EpisodesInfoList[i].SubjectName,
                            year = EpisodesInfoList[i].Year
                        };
                        ExtensionTools.RunCreateNfoFile(movieInfo,
                            Path.Combine(Path.GetDirectoryName(NewFileList[i].FilePath) ?? throw new InvalidOperationException(),
                                Path.GetFileNameWithoutExtension(NewFileList[i].FileName) + ".nfo"));
                    }
                }
            });
            WeakReferenceMessenger.Default.Send<DataSnackbarMessage>(
                new DataSnackbarMessage("重命名", msg, ControlAppearance.Info));
        }

        public void DragOver(IDropInfo dropInfo)
        {
            switch (dropInfo)
            {
                case { Data: DataFilePath, TargetItem: DataFilePath }:
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    dropInfo.Effects = DragDropEffects.All;
                    break;
                }
                case { Data: DataEpisodesInfo, TargetItem: DataEpisodesInfo }:
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    dropInfo.Effects = DragDropEffects.All;
                    break;
                }
                case { Data: DataObject dataObject } when dataObject.GetDataPresent(DataFormats.FileDrop):
                {
                    dropInfo.DropTargetAdorner = null;
                    dropInfo.Effects = DragDropEffects.Copy;
                    break;
                }
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            switch (dropInfo)
            {
                case { Data: DataFilePath sourceItem, TargetItem: DataFilePath targetItem }
                    when SourceFileList.Contains(sourceItem) && SourceFileList.Contains(targetItem) &&
                         !sourceItem.Equals(targetItem):
                {
                    var sourceIndex = SourceFileList.IndexOf(sourceItem);
                    SourceFileList.RemoveAt(sourceIndex);
                    var targetIndex = SourceFileList.IndexOf(targetItem);
                    if (targetIndex < 0 || targetIndex >= SourceFileList.Count) break;

                    if (sourceIndex <= targetIndex)
                    {
                        SourceFileList.Insert(targetIndex + 1, sourceItem);
                    }
                    else
                    {
                        SourceFileList.Insert(targetIndex, sourceItem);
                    }

                    break;
                }
                case { Data: DataEpisodesInfo sourceItem, TargetItem: DataEpisodesInfo targetItem }
                    when EpisodesInfoList.Contains(sourceItem) && EpisodesInfoList.Contains(targetItem) &&
                         !sourceItem.Equals(targetItem):
                {
                    var sourceIndex = EpisodesInfoList.IndexOf(sourceItem);
                    EpisodesInfoList.RemoveAt(sourceIndex);
                    var targetIndex = EpisodesInfoList.IndexOf(targetItem);
                    if (targetIndex < 0 || targetIndex >= EpisodesInfoList.Count) break;

                    if (sourceIndex <= targetIndex)
                    {
                        EpisodesInfoList.Insert(targetIndex + 1, sourceItem);
                    }
                    else
                    {
                        EpisodesInfoList.Insert(targetIndex, sourceItem);
                    }

                    break;
                }
                case { Data: DataObject dataObject } when dataObject.ContainsFileDropList():
                {
                    var fileArray = dataObject.GetFileDropList();
                    var filePathArray = new List<string>();
                    foreach (var file in fileArray)
                    {
                        var addFile = file ?? "";

                        if (File.Exists(addFile))
                        {
                            filePathArray.Add(addFile);
                        }
                        else if (Directory.Exists(addFile))
                        {
                            filePathArray.AddRange(Directory.GetFiles(addFile));
                            filePathArray.AddRange(Directory.GetDirectories(addFile).SelectMany(Directory.GetFiles));
                        }
                        else
                        {
                            Console.WriteLine($"unknow file: {file}");
                        }
                    }

                    filePathArray.Sort(ExtensionTools.StrCmpLogicalW);
                    AddDropFile(filePathArray);

                    break;
                }
            }
        }
    }
}