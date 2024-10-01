using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Ass2Srt;
using BangumiSubReNamer.Models;
using BangumiSubReNamer.Services;
using BangumiSubReNamer.Views.Pages;
using CommunityToolkit.Mvvm.Messaging;
using GongSolutions.Wpf.DragDrop;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace BangumiSubReNamer.ViewModels.Pages
{
    public partial class SubRenamerViewModel : ObservableObject, INavigationAware, IDropTarget
    {
        public SubRenamerViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;

            Console.WriteLine("init SubRenamerViewModel");
        }

        [ObservableProperty] private bool isSelectByExtension = true;
        [ObservableProperty] private ObservableCollection<string> selectExtensions = new();
        [ObservableProperty] private int currentExtension = -1;
        [ObservableProperty] private ObservableCollection<DataFilePath> subFilePaths = new();
        [ObservableProperty] private ObservableCollection<DataFilePath> showSubFilePaths = new();
        [ObservableProperty] private ObservableCollection<DataFilePath> sourceFilePaths = new();
        [ObservableProperty] private ObservableCollection<string> addExtensions = new();
        [ObservableProperty] private string selectAddExtension = "";
        [ObservableProperty] private bool isMoveFile = true;
        [ObservableProperty] private bool isConvAssToSrt = true;
        [ObservableProperty] private Visibility isMovingProcess = Visibility.Hidden;
        [ObservableProperty] private string processText = "";

        private List<DataFilePath> sourceFileSelected = new();
        private List<DataFilePath> showSubFileSelected = new();

        private string subExtensionRegex = "";
        private string subFileEndsRegex = "";
        private string sourceFileEndsRegex = "";

        private readonly INavigationService navigationService;


        [RelayCommand]
        private void OnSelectByExtensionChange()
        {
            if (IsSelectByExtension && SelectExtensions.Count > 0)
            {
                CurrentExtension = 0;
            }
            else
            {
                CurrentExtension = -1;
            }
        }

        [RelayCommand]
        private void OnSourceFilesSelectedItemChange(object sender)
        {
            if (sender is not IList list) return;

            sourceFileSelected = list.Cast<DataFilePath>().ToList();
        }

        [RelayCommand]
        private void OnShowSubFilesSelectedItemChange(object sender)
        {
            if (sender is not IList list) return;

            showSubFileSelected = list.Cast<DataFilePath>().ToList();
        }

        [RelayCommand]
        private void OnDelSourceFilesItem()
        {
            for (var i = sourceFileSelected.Count - 1; i >= 0; i--)
            {
                SourceFilePaths.Remove(sourceFileSelected[i]);
            }
        }

        [RelayCommand]
        private void OnDelShowSubFilesItem()
        {
            for (var i = showSubFileSelected.Count - 1; i >= 0; i--)
            {
                ShowSubFilePaths.Remove(showSubFileSelected[i]);
            }
        }

        [RelayCommand]
        private void OnSortSourceFilesItem()
        {
            var list = SourceFilePaths.ToList();
            list.Sort((pathA, pathB) => ExtensionTools.StrCmpLogicalW(pathA.FilePath, pathB.FilePath));
            SourceFilePaths = new ObservableCollection<DataFilePath>(list);
        }

        [RelayCommand]
        private void OnSortShowSubFilesItem()
        {
            var list = ShowSubFilePaths.ToList();
            list.Sort((pathA, pathB) => ExtensionTools.StrCmpLogicalW(pathA.FilePath, pathB.FilePath));
            ShowSubFilePaths = new ObservableCollection<DataFilePath>(list);
        }


        [RelayCommand]
        private void OnClearAll()
        {
            ShowSubFilePaths.Clear();
            SubFilePaths.Clear();
            SelectExtensions.Clear();
            SourceFilePaths.Clear();
            CurrentExtension = -1;
        }

        [RelayCommand]
        private void OnNavigateToPreviewWindow()
        {
            var sendMsg = new DataFilePathPreview();

            for (int i = 0; i < Math.Min(ShowSubFilePaths.Count, SourceFilePaths.Count); i++)
            {
                if (IsMoveFile)
                {
                    var newPath = Path.GetDirectoryName(SourceFilePaths[i].FilePath);
                    var subFileName = Path.GetFileNameWithoutExtension(SourceFilePaths[i].FilePath);
                    var newFile = newPath + "\\" + subFileName + SelectAddExtension +
                                  Path.GetExtension(ShowSubFilePaths[i].FilePath);

                    sendMsg.fileList.Add(new DataFilePath(newFile, newFile));
                }
                else
                {
                    var newPath = Path.GetDirectoryName(ShowSubFilePaths[i].FilePath);
                    var subFileName = Path.GetFileNameWithoutExtension(SourceFilePaths[i].FilePath);
                    var newFile = newPath + "\\" + subFileName + SelectAddExtension +
                                  Path.GetExtension(ShowSubFilePaths[i].FilePath);

                    sendMsg.fileList.Add(new DataFilePath(newFile, newFile));
                }
            }

            WeakReferenceMessenger.Default.Send<DataFilePathPreview>(sendMsg);
        }

        partial void OnCurrentExtensionChanged(int value)
        {
            Console.WriteLine(CurrentExtension);
            AddShowSubFile();
        }

        public void AddDropFile(List<string> filePathArrayOrder)
        {
            foreach (var filePath in filePathArrayOrder)
            {
                var addDataFilePath = new DataFilePath(filePath: filePath, fileName: Path.GetFileName(filePath));
                if (Regex.IsMatch(Path.GetExtension(filePath), subFileEndsRegex))
                {
                    SubFilePaths.AddUnique(addDataFilePath);
                    var addExtensionName = filePath.GetExtensionName(subExtensionRegex);
                    SelectExtensions.AddUnique(addExtensionName);

                    if (CurrentExtension < 0) CurrentExtension = 0;
                }
                else if (Regex.IsMatch(Path.GetExtension(filePath), sourceFileEndsRegex))
                {
                    SourceFilePaths.AddUnique(addDataFilePath);
                }
                else
                {
                    Console.WriteLine($"unknow add file: {filePath}");
                    WeakReferenceMessenger.Default.Send<DataSnackbarMessage>(
                        new DataSnackbarMessage("未知文件类型" + Path.GetExtension(filePath), filePath, ControlAppearance.Caution));
                }
            }

            AddShowSubFile();
        }

        private void AddShowSubFile()
        {
            ShowSubFilePaths.Clear();
            if (!IsSelectByExtension)
            {
                foreach (var subFilePath in SubFilePaths)
                {
                    ShowSubFilePaths.Add(subFilePath);
                }
            }
            else if (IsSelectByExtension &&
                     SelectExtensions.Count > 0 &&
                     CurrentExtension >= 0 &&
                     CurrentExtension < SelectExtensions.Count)
            {
                string extensionName = SelectExtensions[CurrentExtension];

                foreach (var subFilePath in SubFilePaths)
                {
                    if (!subFilePath.FileName.EndsWith(extensionName)) continue;
                    bool isAdd = true;
                    for (int i = 0; i < SelectExtensions.Count; i++)
                    {
                        if (i != CurrentExtension
                            && subFilePath.FileName.EndsWith(SelectExtensions[i])
                            && SelectExtensions[i].Contains(extensionName))
                        {
                            isAdd = false;
                        }
                    }

                    if (isAdd)
                    {
                        ShowSubFilePaths.Add(subFilePath);
                    }
                }
            }
            else
            {
                Console.WriteLine($"null extension index: {CurrentExtension}");
            }
        }

        [RelayCommand]
        private async Task OnDoReName()
        {
            Console.WriteLine($"current add extension: [{SelectAddExtension}]");
            if (ShowSubFilePaths.Count <= 0)
            {
                return;
            }

            //直接ass转srt
            if (SourceFilePaths.Count <= 0 && ShowSubFilePaths.Count > 0 && IsConvAssToSrt)
            {
                IsMovingProcess = Visibility.Visible;
                ProcessText = "";
                await Task.Run(() =>
                {
                    for (var i = 0; i < ShowSubFilePaths.Count; i++)
                    {
                        var dataFilePath = ShowSubFilePaths[i];
                        if (Path.GetExtension(dataFilePath.FileName) != ".ass") return;

                        var assReader = new AssReader(dataFilePath.FilePath);
                        if (assReader.IsValid())
                        {
                            var srtDialogues = assReader.ReadDialogues();
                            var assAnalyzerForSrt = new AssAnalyzerForSrt(srtDialogues, 2, false);
                            var srtLines = assAnalyzerForSrt.Analyze();
                            var fileDirNoAss = IsSelectByExtension
                                ? dataFilePath.FilePath.Replace(SelectExtensions[CurrentExtension], "")
                                : dataFilePath.FilePath.Replace(".ass", "");

                            var fileStream = new FileStream(fileDirNoAss + SelectAddExtension + ".srt", FileMode.OpenOrCreate);
                            var writer = new StreamWriter(fileStream, Encoding.UTF8);
                            foreach (var srtLine in srtLines)
                            {
                                writer.WriteLine(srtLine);
                            }

                            writer.Flush();
                            writer.Close();
                        }


                        ProcessText = $"{i + 1}/{ShowSubFilePaths.Count}";
                    }
                });
            }
            //根据视频重命名
            else if (SourceFilePaths.Count > 0 && ShowSubFilePaths.Count > 0)
            {
                IsMovingProcess = Visibility.Visible;
                ProcessText = "";
                await Task.Run(() =>
                {
                    var count = Math.Min(ShowSubFilePaths.Count, SourceFilePaths.Count);
                    for (int i = 0; i < count; i++)
                    {
                        if (!File.Exists(ShowSubFilePaths[i].FilePath)) continue;
                        if (IsMoveFile)
                        {
                            var newPath = Path.GetDirectoryName(SourceFilePaths[i].FilePath) ?? "";
                            var subFileName = Path.GetFileNameWithoutExtension(SourceFilePaths[i].FilePath);
                            if (IsConvAssToSrt)
                            {
                                var dataFilePath = ShowSubFilePaths[i];
                                if (Path.GetExtension(dataFilePath.FileName) != ".ass") return;

                                var assReader = new AssReader(dataFilePath.FilePath);
                                if (assReader.IsValid())
                                {
                                    var srtDialogues = assReader.ReadDialogues();
                                    var assAnalyzerForSrt = new AssAnalyzerForSrt(srtDialogues, 2, false);
                                    var srtLines = assAnalyzerForSrt.Analyze();

                                    var fileStream = new FileStream(Path.Combine(newPath, subFileName) + SelectAddExtension + ".srt", FileMode.OpenOrCreate);
                                    var writer = new StreamWriter(fileStream, Encoding.UTF8);
                                    foreach (var srtLine in srtLines)
                                    {
                                        writer.WriteLine(srtLine);
                                    }

                                    writer.Flush();
                                    writer.Close();
                                }
                            }
                            else
                            {

                                var newFile = Path.Combine(newPath, subFileName) + SelectAddExtension + Path.GetExtension(ShowSubFilePaths[i].FilePath);

                                File.Copy(ShowSubFilePaths[i].FilePath, newFile, true);
                                Console.WriteLine(newFile);
                            }
                        }
                        else
                        {
                            var newPath = Path.GetDirectoryName(ShowSubFilePaths[i].FilePath) ?? "";
                            var subFileName = Path.GetFileNameWithoutExtension(SourceFilePaths[i].FilePath);
                            if (IsConvAssToSrt)
                            {
                                var dataFilePath = ShowSubFilePaths[i];
                                if (Path.GetExtension(dataFilePath.FileName) != ".ass") return;

                                var assReader = new AssReader(dataFilePath.FilePath);
                                if (assReader.IsValid())
                                {
                                    var srtDialogues = assReader.ReadDialogues();
                                    var assAnalyzerForSrt = new AssAnalyzerForSrt(srtDialogues, 2, false);
                                    var srtLines = assAnalyzerForSrt.Analyze();

                                    var fileStream = new FileStream(Path.Combine(newPath, subFileName) + SelectAddExtension + ".srt", FileMode.OpenOrCreate);
                                    var writer = new StreamWriter(fileStream, Encoding.UTF8);
                                    foreach (var srtLine in srtLines)
                                    {
                                        writer.WriteLine(srtLine);
                                    }

                                    writer.Flush();
                                    writer.Close();
                                }
                            }
                            else
                            {
                                var newFile = Path.Combine(newPath, subFileName) + SelectAddExtension + Path.GetExtension(ShowSubFilePaths[i].FilePath);
                                File.Move(ShowSubFilePaths[i].FilePath, newFile, true);
                                Console.WriteLine(newFile);
                            }
                        }

                        ProcessText = $"{i + 1}/{count}";
                    }

                    // Thread.Sleep(500);
                });
            }

            for (int i = SubFilePaths.Count - 1; i >= 0; i--)
            {
                if (ShowSubFilePaths.Contains(SubFilePaths[i]))
                {
                    SubFilePaths.RemoveAt(i);
                }
            }

            ShowSubFilePaths.Clear();
            IsMovingProcess = Visibility.Hidden;
        }

        partial void OnSelectAddExtensionChanged(string value)
        {
            Console.WriteLine($"add extension: [{SelectAddExtension}]");
        }

        public void OnNavigatedTo()
        {
            AddExtensions = GlobalConfig.Instance.ReNamerConfig.DefaultAddExtensions.ConvertToObservableCollection();
            subFileEndsRegex = GlobalConfig.Instance.ReNamerConfig.AddSubFileExtensionRegex;
            sourceFileEndsRegex = GlobalConfig.Instance.ReNamerConfig.AddSourceFileExtensionRegex;
            subExtensionRegex = GlobalConfig.Instance.ReNamerConfig.SubFileExtensionRegex;

            // IsMovingProcess = Visibility.Hidden;
            // Height = GlobalConfig.Instance.Height - 70;
        }

        public void OnNavigatedFrom()
        {
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
                    when SourceFilePaths.Contains(sourceItem) && SourceFilePaths.Contains(targetItem) &&
                         !sourceItem.Equals(targetItem):
                {
                    var sourceIndex = SourceFilePaths.IndexOf(sourceItem);
                    SourceFilePaths.RemoveAt(sourceIndex);
                    var targetIndex = SourceFilePaths.IndexOf(targetItem);
                    if (targetIndex < 0 || targetIndex >= SourceFilePaths.Count) break;

                    if (sourceIndex <= targetIndex)
                    {
                        SourceFilePaths.Insert(targetIndex + 1, sourceItem);
                    }
                    else
                    {
                        SourceFilePaths.Insert(targetIndex, sourceItem);
                    }

                    break;
                }
                case { Data: DataFilePath sourceItem, TargetItem: DataFilePath targetItem }
                    when ShowSubFilePaths.Contains(sourceItem) && ShowSubFilePaths.Contains(targetItem) &&
                         !sourceItem.Equals(targetItem):
                {
                    var sourceIndex = ShowSubFilePaths.IndexOf(sourceItem);
                    ShowSubFilePaths.RemoveAt(sourceIndex);
                    var targetIndex = ShowSubFilePaths.IndexOf(targetItem);
                    if (targetIndex < 0 || targetIndex >= ShowSubFilePaths.Count) break;

                    if (sourceIndex <= targetIndex)
                    {
                        ShowSubFilePaths.Insert(targetIndex + 1, sourceItem);
                    }
                    else
                    {
                        ShowSubFilePaths.Insert(targetIndex, sourceItem);
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