using BangumiMediaTool.Models;
using BangumiMediaTool.Services.Program;
using CommunityToolkit.Mvvm.Messaging;
using Wpf.Ui.Controls;

namespace BangumiMediaTool.ViewModels.Pages;

public partial class SettingsViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty] private AppConfig _appConfig = new();

    public void OnNavigatedTo()
    {
        AppConfig = GlobalConfig.Instance.AppConfig;
    }

    public void OnNavigatedFrom() { }

    [RelayCommand]
    public void OnSetConfig()
    {
        GlobalConfig.Instance.WriteConfig(AppConfig);
        WeakReferenceMessenger.Default.Send(new DataSnackbarMessage("更新设置", string.Empty, ControlAppearance.Success));
    }
}