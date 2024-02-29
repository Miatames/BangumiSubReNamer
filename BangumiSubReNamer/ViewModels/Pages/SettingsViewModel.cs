using BangumiSubReNamer.Models;
using BangumiSubReNamer.Services;
using CommunityToolkit.Mvvm.Messaging;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace BangumiSubReNamer.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, IRecipient<DataWindowSize>, INavigationAware
    {
        public SettingsViewModel()
        {
            WeakReferenceMessenger.Default.Register<DataWindowSize>(this);
        }

        [ObservableProperty] private int height = 580;

        [ObservableProperty] private string sourceFileExtensions;
        [ObservableProperty] private string subFileExtensions;
        [ObservableProperty] private string defaultAddFileExtensions;
        [ObservableProperty] private string subFileExtensionRegex;

        [RelayCommand]
        private void OnSetReNamerConfig()
        {
            var reNamerConfig = new DataReNamerConfig(
                subFileExtensions: SubFileExtensions,
                sourceFileExtensions: SourceFileExtensions,
                defaultAddExtensions: DefaultAddFileExtensions,
                subFileExtensionRegex: SubFileExtensionRegex);

            GlobalConfig.Instance.ReNamerConfig = reNamerConfig;
            
            GlobalConfig.Instance.WriteConfig(reNamerConfig);

            WeakReferenceMessenger.Default.Send(new DataSnackbarMessage("更新设置",
                $"{SubFileExtensions}  {SourceFileExtensions}  {DefaultAddFileExtensions}  {SubFileExtensionRegex}",
                ControlAppearance.Success));
        }

        public void Receive(DataWindowSize message)
        {
            Height = message.Height - 70;
        }


        public void OnNavigatedTo()
        {
            SourceFileExtensions = GlobalConfig.Instance.ReNamerConfig.SourceFileExtensions;
            SubFileExtensions = GlobalConfig.Instance.ReNamerConfig.SubFileExtensions;
            DefaultAddFileExtensions = GlobalConfig.Instance.ReNamerConfig.DefaultAddExtensions;
            SubFileExtensionRegex = GlobalConfig.Instance.ReNamerConfig.SubFileExtensionRegex;
        }

        public void OnNavigatedFrom()
        {
        }
    }
}