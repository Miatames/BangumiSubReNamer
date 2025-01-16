using BangumiMediaTool.ViewModels.Windows;
using Wpf.Ui.Appearance;

namespace BangumiMediaTool.Views.Windows;

public partial class ConsoleLogWindow
{
    public ConsoleLogWindow()
    {
        DataContext = this;

        SystemThemeWatcher.Watch(this);

        InitializeComponent();
    }
}