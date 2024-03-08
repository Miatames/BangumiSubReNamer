using System.Windows.Controls;
using System.Windows.Input;
using BangumiSubReNamer.Models;
using BangumiSubReNamer.ViewModels.Pages;
using CommunityToolkit.Mvvm.Messaging;
using Wpf.Ui.Controls;

namespace BangumiSubReNamer.Views.Pages
{
    public partial class MediaDataPage : INavigableView<MediaDataViewModel>
    {
        public MediaDataViewModel ViewModel { get; }

        public MediaDataPage(MediaDataViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            
            InitializeComponent();
        }

        private void SendToRename_OnClick(object sender, RoutedEventArgs e)
        {
            var infoList = new DataEpisodesInfoList();

            if (UI_EpisodesInfoList.SelectedItems.Count == 0)
            {
                foreach (var item in UI_EpisodesInfoList.Items)
                {
                    if (item is not DataEpisodesInfo info)
                    {
                        Console.WriteLine("not info");
                        continue;
                    }

                    infoList.Infos.Add(info);
                }
            }
            else
            {
                foreach (var obj in UI_EpisodesInfoList.SelectedItems)
                {
                    if (obj is not DataEpisodesInfo info)
                    {
                        Console.WriteLine("not info");
                        continue;
                    }

                    infoList.Infos.Add(info);
                }
            }

            WeakReferenceMessenger.Default.Send<DataEpisodesInfoList>(infoList);
            WeakReferenceMessenger.Default.Send<DataSnackbarMessage>(
                new DataSnackbarMessage("添加到元数据", "", ControlAppearance.Success));
        }

        private void UI_SearchTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            //enter后移除焦点 使文本可以被获取到
            if (e.Key == Key.Enter)
            {
                UI_SearchTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
    }
}