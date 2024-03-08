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

            Style itemContainerStyleEpisodes = new Style(typeof(ListViewItem));
            itemContainerStyleEpisodes.Setters.Add(new Setter(ListViewItem.AllowDropProperty, true));
            itemContainerStyleEpisodes.Setters.Add(new EventSetter(ListViewItem.PreviewMouseLeftButtonDownEvent,
                new MouseButtonEventHandler(OnPreviewMouseLeftButtonDown)));
            itemContainerStyleEpisodes.Setters.Add(new EventSetter(ListViewItem.DropEvent,
                new DragEventHandler(OnEpisodesListItemDrag)));
            UI_EpisodesInfoList.ItemContainerStyle = itemContainerStyleEpisodes;

            Style itemContainerStyleSource = new Style(typeof(ListViewItem));
            itemContainerStyleSource.Setters.Add(new Setter(ListViewItem.AllowDropProperty, true));
            itemContainerStyleSource.Setters.Add(new EventSetter(ListViewItem.PreviewMouseLeftButtonDownEvent,
                new MouseButtonEventHandler(OnPreviewMouseLeftButtonDown)));
            itemContainerStyleSource.Setters.Add(new EventSetter(ListViewItem.DropEvent,
                new DragEventHandler(OnSourceListItemDrag)));
            UI_SourceFileList.ItemContainerStyle = itemContainerStyleSource;

            Style itemContainerStyleNew = new Style(typeof(ListViewItem));
            UI_NewFileList.ItemContainerStyle = itemContainerStyleNew;
        }

        private void OnSourceListItemDrag(object sender, DragEventArgs e)
        {
            DataFilePath? droppedDataFilePath = e.Data.GetData(typeof(DataFilePath)) as DataFilePath;
            DataFilePath? target = ((ListBoxItem)(sender)).DataContext as DataFilePath;

            if (droppedDataFilePath == null || target == null) return;

            int removedIdx = UI_SourceFileList.Items.IndexOf(droppedDataFilePath);
            int targetIdx = UI_SourceFileList.Items.IndexOf(target);

            if (removedIdx < targetIdx)
            {
                ViewModel.SourceFileList.Insert(targetIdx + 1, droppedDataFilePath);
                ViewModel.SourceFileList.RemoveAt(removedIdx);
            }
            else
            {
                int remIdx = removedIdx + 1;
                if (ViewModel.SourceFileList.Count + 1 > remIdx)
                {
                    ViewModel.SourceFileList.Insert(targetIdx, droppedDataFilePath);
                    ViewModel.SourceFileList.RemoveAt(remIdx);
                }
            }
        }

        private void OnEpisodesListItemDrag(object sender, DragEventArgs e)
        {
            DataEpisodesInfo? droppedDataFilePath = e.Data.GetData(typeof(DataEpisodesInfo)) as DataEpisodesInfo;
            DataEpisodesInfo? target = ((ListBoxItem)(sender)).DataContext as DataEpisodesInfo;

            if (droppedDataFilePath == null || target == null) return;

            int removedIdx = UI_EpisodesInfoList.Items.IndexOf(droppedDataFilePath);
            int targetIdx = UI_EpisodesInfoList.Items.IndexOf(target);

            if (removedIdx < targetIdx)
            {
                ViewModel.EpisodesInfoList.Insert(targetIdx + 1, droppedDataFilePath);
                ViewModel.EpisodesInfoList.RemoveAt(removedIdx);
            }
            else
            {
                int remIdx = removedIdx + 1;
                if (ViewModel.EpisodesInfoList.Count + 1 > remIdx)
                {
                    ViewModel.EpisodesInfoList.Insert(targetIdx, droppedDataFilePath);
                    ViewModel.EpisodesInfoList.RemoveAt(remIdx);
                }
            }
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem draggedItem)
            {
                DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
                draggedItem.IsSelected = true;
            }
        }

        private void UI_MainGrid_OnDrop(object sender, DragEventArgs e)
        {
            var fileArray = e.Data?.GetData(DataFormats.FileDrop) as Array;
            if (fileArray == null) return;
            var filePathArray = new List<string>();
            foreach (var file in fileArray)
            {
                var addFile = file.ToString() ?? "";

                if (File.Exists(addFile))
                {
                    filePathArray.Add(addFile);
                }
                else if (Directory.Exists(addFile))
                {
                    filePathArray.AddRange(Directory.GetFiles(addFile));
                }
                else
                {
                    Console.WriteLine($"unknow file: {file}");
                }
            }

            var filePathArrayOrder = from file in filePathArray
                orderby file
                select file;

            foreach (var s in filePathArrayOrder)
            {
                Console.WriteLine(s);
            }

            ViewModel.AddDropFile(filePathArrayOrder.ToList());
        }
    }
}