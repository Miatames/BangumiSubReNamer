using BangumiMediaTool.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace BangumiMediaTool.Views.Pages;

public partial class SettingsPage : INavigableView<SettingsViewModel>
{
    public SettingsViewModel ViewModel { get; private set; }

    public SettingsPage(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}