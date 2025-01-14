using System.Collections.ObjectModel;
using BangumiMediaTool.Models;

namespace BangumiMediaTool.ViewModels.Windows;

public partial class FilePreviewWindowViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<DataFilePath> paths;

    public FilePreviewWindowViewModel(List<DataFilePath> dataFilePaths)
    {
        paths = new ObservableCollection<DataFilePath>(dataFilePaths);
    }
}
