using BangumiMediaTool.ViewModels.Windows;
using Wpf.Ui.Appearance;

namespace BangumiMediaTool.Views.Windows;

public partial class FilePreviewWindow
{
    public FilePreviewWindowViewModel ViewModel { get; }

    public FilePreviewWindow(
        FilePreviewWindowViewModel viewModel
    )
    {
        ViewModel = viewModel;
        DataContext = this;

        SystemThemeWatcher.Watch(this);

        InitializeComponent();
    }
}
