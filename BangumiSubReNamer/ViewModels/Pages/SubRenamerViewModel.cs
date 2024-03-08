using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using BangumiSubReNamer.Models;
using BangumiSubReNamer.Services;
using CommunityToolkit.Mvvm.Messaging;
using Wpf.Ui.Controls;

namespace BangumiSubReNamer.ViewModels.Pages
{
    public partial class SubRenamerViewModel : ObservableObject, INavigationAware
        , IRecipient<DataWindowSize>, IRecipient<DataFilePath>
    {
        public SubRenamerViewModel()
        {
            WeakReferenceMessenger.Default.Register<DataWindowSize>(this);

            Console.WriteLine("init SubRenamerViewModel");
        }

        [ObservableProperty] private bool isSelectByExtension = true;
        [ObservableProperty] private ObservableCollection<string> selectExtensions = new();
        [ObservableProperty] private int currentExtension = -1;
        [ObservableProperty] private ObservableCollection<DataFilePath> subFilePaths = new();
        [ObservableProperty] private ObservableCollection<DataFilePath> showSubFilePaths = new();
        [ObservableProperty] private ObservableCollection<DataFilePath> sourceFilePaths = new();
        [ObservableProperty] private ObservableCollection<string> addExtensions = new();
        [ObservableProperty] private string selectAddExtension;
        [ObservableProperty] private int height = 580;
        [ObservableProperty] private bool isMoveFile = true;
        [ObservableProperty] private Visibility isMovingProcess = Visibility.Hidden;

        private string subExtensionRegex = "";
        private string subFileEndsRegex = "";
        private string sourceFileEndsRegex = "";


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
        private void OnClearAll()
        {
            ShowSubFilePaths.Clear();
            SubFilePaths.Clear();
            SelectExtensions.Clear();
            SourceFilePaths.Clear();
            CurrentExtension = -1;
        }

        partial void OnCurrentExtensionChanged(int value)
        {
            Console.WriteLine(CurrentExtension);
            AddShowSubFile();
        }

        public void RemoveFromSubList(string file)
        {
            foreach (var showSubFilePath in ShowSubFilePaths)
            {
                if (showSubFilePath.FileName == file)
                {
                    ShowSubFilePaths.Remove(showSubFilePath);
                    break;
                }
            }
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
        private async void OnDoReName()
        {
            Console.WriteLine($"current add extension: [{SelectAddExtension}]");
            if (ShowSubFilePaths.Count <= 0)
            {
                return;
            }

            IsMovingProcess = Visibility.Visible;
            await Task.Run(() =>
            {
                for (int i = 0; i < Math.Min(ShowSubFilePaths.Count, SourceFilePaths.Count); i++)
                {
                    if (!File.Exists(ShowSubFilePaths[i].FilePath)) continue;
                    if (IsMoveFile)
                    {
                        var newPath = Path.GetDirectoryName(SourceFilePaths[i].FilePath);
                        var subFileName = Path.GetFileNameWithoutExtension(SourceFilePaths[i].FilePath);
                        var newFile = newPath + "\\" + subFileName + SelectAddExtension +
                                      Path.GetExtension(ShowSubFilePaths[i].FilePath);

                        File.Copy(ShowSubFilePaths[i].FilePath, newFile, true);

                        Console.WriteLine(newFile);
                    }
                    else
                    {
                        var newPath = Path.GetDirectoryName(ShowSubFilePaths[i].FilePath);
                        var subFileName = Path.GetFileNameWithoutExtension(SourceFilePaths[i].FilePath);
                        var newFile = newPath + "\\" + subFileName + SelectAddExtension +
                                      Path.GetExtension(ShowSubFilePaths[i].FilePath);
                        
                        File.Move(ShowSubFilePaths[i].FilePath, newFile, true);

                        Console.WriteLine(newFile);
                    }
                }

                // Thread.Sleep(500);
            });

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

        public void Receive(DataWindowSize message)
        {
            Height = message.Height - 70;
        }

        public void Receive(DataFilePath message)
        {
            ShowSubFilePaths.Add(message);
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
            Height = GlobalConfig.Instance.Height - 70;
        }

        public void OnNavigatedFrom() { }
    }
}