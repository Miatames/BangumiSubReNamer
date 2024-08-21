using BangumiSubReNamer.Models;
using BangumiSubReNamer.Services;
using CommunityToolkit.Mvvm.Messaging;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace BangumiSubReNamer.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        public SettingsViewModel()
        {
            Console.WriteLine("init SettingsViewModel");
        }

        [ObservableProperty] private string addSourceFileExtensionRegex;
        [ObservableProperty] private string addSubFileExtensionRegex;
        [ObservableProperty] private string defaultAddFileExtensions;
        [ObservableProperty] private string subFileExtensionRegex;
        [ObservableProperty] private string outFilePath;
        [ObservableProperty] private string bangumiFileTemplate;
        [ObservableProperty] private string movieFileTemplate;
        [ObservableProperty] private string qbtWebUrl;
        [ObservableProperty] private string qbtDownloadPath;

        [RelayCommand]
        private void OnSetReNamerConfig()
        {
            if (OutFilePath != @"{RootPath}" && !OutFilePath.EndsWith(@"\"))
            {
                OutFilePath += @"\";
            }

            var reNamerConfig = new DataReNamerConfig(
                addSubFileExtensionRegex: AddSubFileExtensionRegex,
                addSourceFileExtensionRegex: AddSourceFileExtensionRegex,
                defaultAddExtensions: DefaultAddFileExtensions,
                subFileExtensionRegex: SubFileExtensionRegex);

            GlobalConfig.Instance.ReNamerConfig = reNamerConfig;
            GlobalConfig.Instance.OutFilePath = OutFilePath;
            GlobalConfig.Instance.CreateFileNameTemplateBangumi = BangumiFileTemplate;
            GlobalConfig.Instance.CreateFileNameTemplateMovie = MovieFileTemplate;
            GlobalConfig.Instance.QbtWebUrl = QbtWebUrl;
            GlobalConfig.Instance.QbtDownloadPath = QbtDownloadPath;
            GlobalConfig.Instance.WriteConfig();

            WeakReferenceMessenger.Default.Send(new DataSnackbarMessage("更新设置", "", ControlAppearance.Success));
        }

        public void OnNavigatedTo()
        {
            AddSourceFileExtensionRegex = GlobalConfig.Instance.ReNamerConfig.AddSourceFileExtensionRegex;
            AddSubFileExtensionRegex = GlobalConfig.Instance.ReNamerConfig.AddSubFileExtensionRegex;
            DefaultAddFileExtensions = GlobalConfig.Instance.ReNamerConfig.DefaultAddExtensions;
            SubFileExtensionRegex = GlobalConfig.Instance.ReNamerConfig.SubFileExtensionRegex;
            OutFilePath = GlobalConfig.Instance.OutFilePath;
            BangumiFileTemplate = GlobalConfig.Instance.CreateFileNameTemplateBangumi;
            MovieFileTemplate = GlobalConfig.Instance.CreateFileNameTemplateMovie;
            QbtWebUrl = GlobalConfig.Instance.QbtWebUrl;
            QbtDownloadPath = GlobalConfig.Instance.QbtDownloadPath;
        }

        public void OnNavigatedFrom()
        {
        }
    }
}