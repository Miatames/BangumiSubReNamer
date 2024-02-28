using System.Collections.ObjectModel;
using BangumiSubReNamer.Models;
using BangumiSubReNamer.ViewModels.Pages;
using BangumiSubReNamer.Views.Pages;
using CommunityToolkit.Mvvm.Messaging;
using Wpf.Ui.Controls;

namespace BangumiSubReNamer.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty] private string _applicationTitle = "BangumiSubReNamer";

        [ObservableProperty] private int width = 1100;
        [ObservableProperty] private int height = 650;

        [ObservableProperty] private ObservableCollection<object> _menuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "重命名",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Rename24 },
                TargetPageType = typeof(Views.Pages.RenamerPage)
            },
            new NavigationViewItem()
            {
                Content = "设置",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Views.Pages.SettingsPage)
            }
        };

        [ObservableProperty] private ObservableCollection<object> _footerMenuItems = new()
        {
            
        };

        [ObservableProperty] private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Home", Tag = "tray_home" }
        };

        partial void OnHeightChanged(int value)
        {
            Console.WriteLine($"window size change: {Width}--{Height}");

            WeakReferenceMessenger.Default.Send(new DataWindowSize(Width, Height));
        }
    }
}