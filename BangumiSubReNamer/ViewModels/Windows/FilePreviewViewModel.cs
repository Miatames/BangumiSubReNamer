using System.Collections.ObjectModel;
using BangumiSubReNamer.Models;
using CommunityToolkit.Mvvm.Messaging;
using Wpf.Ui.Controls;

namespace BangumiSubReNamer.ViewModels.Windows;

public partial class FilePreviewWindowViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<DataFilePath> paths = new();

    public FilePreviewWindowViewModel(DataFilePathPreview dataFilePathPreview)
    {
        paths.Clear();
        foreach (var dataFilePath in dataFilePathPreview.fileList)
        {
            paths.Add(new DataFilePath(dataFilePath.FilePath, dataFilePath.FileName));
        }
    }
}