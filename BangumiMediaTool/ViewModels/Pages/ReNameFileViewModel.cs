using System.Collections;
using System.Collections.ObjectModel;
using BangumiMediaTool.Models;
using BangumiMediaTool.Services.Page;
using BangumiMediaTool.Services.Program;
using BangumiMediaTool.ViewModels.Windows;
using BangumiMediaTool.Views.Windows;
using CommunityToolkit.Mvvm.Messaging;
using GongSolutions.Wpf.DragDrop;
using NaturalSort.Extension;
using Wpf.Ui.Controls;

namespace BangumiMediaTool.ViewModels.Pages;

public partial class ReNameFileViewModel : ObservableObject, INavigationAware, IDropTarget
{
    [ObservableProperty] private bool _isSelectByExtension = true;
    [ObservableProperty] private ObservableCollection<string> _selectExtensions = [];
    [ObservableProperty] private int _currentExtension = -1;
    [ObservableProperty] private ObservableCollection<DataFilePath> _subFilePaths = [];
    [ObservableProperty] private ObservableCollection<DataFilePath> _showSubFilePaths = [];
    [ObservableProperty] private ObservableCollection<DataFilePath> _sourceFilePaths = [];
    [ObservableProperty] private ObservableCollection<string> _addExtensions = [];
    [ObservableProperty] private string _selectAddExtension = string.Empty;
    [ObservableProperty] private int _currentFileOperateMode = 0;

    private List<DataFilePath> sourceFileSelected = [];
    private List<DataFilePath> showSubFileSelected = [];

    public void OnNavigatedTo()
    {
        AddExtensions = GlobalConfig.Instance.AppConfig.DefaultAddSubtitleFilesExtensions.ConvertToObservableCollection();
    }

    public void OnNavigatedFrom() { }

    #region DragDrop

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
                when SourceFilePaths.Contains(sourceItem)
                     && SourceFilePaths.Contains(targetItem)
                     && !sourceItem.Equals(targetItem):
            {
                SourceFilePaths.DragDropListItem(sourceItem, targetItem);
                break;
            }
            case { Data: DataFilePath sourceItem, TargetItem: DataFilePath targetItem }
                when ShowSubFilePaths.Contains(sourceItem)
                     && ShowSubFilePaths.Contains(targetItem)
                     && !sourceItem.Equals(targetItem):
            {
                ShowSubFilePaths.DragDropListItem(sourceItem, targetItem);
                break;
            }
            case { Data: DataObject dataObject } when dataObject.ContainsFileDropList():
            {
                var files = dataObject.GetFileDropList();
                var (mediaFiles, subFiles) = files.GetDropFileList();
                mediaFiles.ForEach(item => SourceFilePaths.AddUnique(item));
                OnDropSubFiles(subFiles);
                break;
            }
        }
    }


    /// <summary>
    /// 处理拖入的字幕文件
    /// </summary>
    /// <param name="files">文件列表</param>
    public void OnDropSubFiles(List<DataFilePath> files)
    {
        var regexSubEx = GlobalConfig.Instance.AppConfig.RegexMatchSubtitleFiles;

        foreach (var file in files)
        {
            SubFilePaths.AddUnique(file);
            var addExName = file.FileName.GetExtensionName(regexSubEx);
            SelectExtensions.AddUnique(addExName);
            if (CurrentExtension < 0) CurrentExtension = 0;
        }

        AddShowSubFiles();
    }

    /// <summary>
    /// 添加媒体文件
    /// </summary>
    /// <param name="files">文件列表</param>
    public void AddSourceFiles(List<DataFilePath> files)
    {
        OnClearAll();
        SourceFilePaths = new ObservableCollection<DataFilePath>(files);
    }

    #endregion

    partial void OnCurrentExtensionChanged(int value)
    {
        Logs.LogInfo($"OnCurrentExtensionChanged: {value}");
        AddShowSubFiles();
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
    private void OnClearSourceFileList()
    {
        SourceFilePaths.Clear();
    }

    [RelayCommand]
    private void OnClearShowSubFileList()
    {
        ShowSubFilePaths.Clear();
    }

    [RelayCommand]
    private void OnSortSourceFileList()
    {
        var list = SourceFilePaths.ToList()
            .OrderBy(path => path.FilePath, StringComparer.OrdinalIgnoreCase.WithNaturalSort());
        SourceFilePaths = new ObservableCollection<DataFilePath>(list);
    }

    [RelayCommand]
    private void OnSortShowSubFileList()
    {
        var list = ShowSubFilePaths.ToList()
            .OrderBy(path => path.FilePath, StringComparer.OrdinalIgnoreCase.WithNaturalSort());
        ShowSubFilePaths = new ObservableCollection<DataFilePath>(list);
    }

    /// <summary>
    /// 筛选所有字幕文件添加到列表
    /// </summary>
    private void AddShowSubFiles()
    {
        ShowSubFilePaths.Clear();
        if (!IsSelectByExtension)
        {
            ShowSubFilePaths = new ObservableCollection<DataFilePath>(SubFilePaths);
        }
        else
        {
            var list = SubFilePaths.ToList().GetMatchSubtitleFiles(CurrentExtension, SelectExtensions.ToList());
            ShowSubFilePaths = new ObservableCollection<DataFilePath>(list);
        }
    }

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
        SelectExtensions.Clear();
        SourceFilePaths.Clear();
        SubFilePaths.Clear();
        ShowSubFilePaths.Clear();
        CurrentExtension = -1;
    }


    [RelayCommand]
    private void OnNavigateToPreviewWindow()
    {
        var currentExtensionStr = string.Empty;
        if (CurrentExtension >= 0 && CurrentExtension < SelectExtensions.Count) currentExtensionStr = SelectExtensions[CurrentExtension];

        var list = ReNameFileService.CreateNewFilePaths(SourceFilePaths.ToList(), ShowSubFilePaths.ToList(),
            currentExtensionStr, SelectAddExtension, CurrentFileOperateMode);
        var window = new FilePreviewWindow(new FilePreviewWindowViewModel(list))
        {
            Owner = App.GetService<MainWindow>(),
            ShowInTaskbar = false
        };
        window.ShowDialog();
    }

    [RelayCommand]
    private async Task OnRunFileOperate()
    {
        var main = App.GetService<MainWindowViewModel>();
        main?.SetGlobalProcess(true);

        var currentExtensionStr = string.Empty;
        if (CurrentExtension >= 0 && CurrentExtension < SelectExtensions.Count) currentExtensionStr = SelectExtensions[CurrentExtension];
        var list = ReNameFileService.CreateNewFilePaths(SourceFilePaths.ToList(), ShowSubFilePaths.ToList(),
            currentExtensionStr, SelectAddExtension, CurrentFileOperateMode);
        var record = await ReNameFileService.RunFileOperates(ShowSubFilePaths.ToList(), list, CurrentFileOperateMode);

        main?.SetGlobalProcess(false);
        if (!string.IsNullOrEmpty(record))
        {
            WeakReferenceMessenger.Default.Send(new DataSnackbarMessage(
                "完成",
                record,
                ControlAppearance.Info));
        }
    }
}