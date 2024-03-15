using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using BangumiSubReNamer.Models;
using BangumiSubReNamer.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace BangumiSubReNamer.Views.Pages
{
    public partial class SubRenamerPage : INavigableView<SubRenamerViewModel>
    {
        public SubRenamerViewModel ViewModel { get; }

        public SubRenamerPage(SubRenamerViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}