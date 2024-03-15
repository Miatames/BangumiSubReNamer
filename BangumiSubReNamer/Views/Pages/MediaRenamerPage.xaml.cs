using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using BangumiSubReNamer.Models;
using BangumiSubReNamer.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace BangumiSubReNamer.Views.Pages
{
    public partial class MediaRenamerPage : INavigableView<MediaRenamerViewModel>
    {
        public MediaRenamerViewModel ViewModel { get; }

        public MediaRenamerPage(MediaRenamerViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}