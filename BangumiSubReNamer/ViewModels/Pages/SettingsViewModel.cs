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
            GlobalConfig.Instance.WriteConfig();

            WeakReferenceMessenger.Default.Send(new DataSnackbarMessage("更新设置",
                AddSubFileExtensionRegex + "\n" +
                AddSourceFileExtensionRegex + "\n" +
                DefaultAddFileExtensions + "\n" +
                SubFileExtensionRegex + "\n" +
                OutFilePath,
                ControlAppearance.Success));
        }

        public void OnNavigatedTo()
        {
            AddSourceFileExtensionRegex = GlobalConfig.Instance.ReNamerConfig.AddSourceFileExtensionRegex;
            AddSubFileExtensionRegex = GlobalConfig.Instance.ReNamerConfig.AddSubFileExtensionRegex;
            DefaultAddFileExtensions = GlobalConfig.Instance.ReNamerConfig.DefaultAddExtensions;
            SubFileExtensionRegex = GlobalConfig.Instance.ReNamerConfig.SubFileExtensionRegex;
            OutFilePath = GlobalConfig.Instance.OutFilePath;

            // Height = GlobalConfig.Instance.Height - 70;
        }

        public void OnNavigatedFrom() { }
    }
}