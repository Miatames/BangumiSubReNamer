using System.IO;
using BangumiSubReNamer.Models;
using BangumiSubReNamer.Services;
using BangumiSubReNamer.ViewModels.Pages;
using BangumiSubReNamer.ViewModels.Windows;
using BangumiSubReNamer.Views.Pages;
using CommunityToolkit.Mvvm.Messaging;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace BangumiSubReNamer.Views.Windows
{
    public partial class FilePreviewWindow
    {
        public FilePreviewWindowViewModel ViewModel { get; }

        public FilePreviewWindow(
            FilePreviewWindowViewModel viewModel
        )
        {
            ViewModel = viewModel;
            DataContext = this;

            SystemThemeWatcher.Watch(this);

            InitializeComponent();
        }
    }
}