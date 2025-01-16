using BangumiMediaTool.Models;
using BangumiMediaTool.Services.Program;
using BangumiMediaTool.Views.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Wpf.Ui.Controls;

namespace BangumiMediaTool.ViewModels.Pages;

public partial class SettingsViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty] private AppConfig _appConfig = new();

    private ConsoleLogWindow? consoleLogWindow = null;

    public void OnNavigatedTo()
    {
        AppConfig = GlobalConfig.Instance.AppConfig;
    }

    public void OnNavigatedFrom() { }

    [RelayCommand]
    private void OnSetConfig()
    {
        GlobalConfig.Instance.WriteConfig(AppConfig);
        WeakReferenceMessenger.Default.Send(new DataSnackbarMessage("更新设置", string.Empty, ControlAppearance.Success));
    }

    [RelayCommand]
    private void OnShowConsoleLogWindow()
    {
        if (consoleLogWindow != null) return;

        consoleLogWindow = new ConsoleLogWindow()
        {
            ShowInTaskbar = false
        };
        consoleLogWindow.Show();
        consoleLogWindow.Closed += (sender, args) => consoleLogWindow = null;
    }
}