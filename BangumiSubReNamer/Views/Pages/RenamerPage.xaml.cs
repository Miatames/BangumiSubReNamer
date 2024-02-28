using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using BangumiSubReNamer.Models;
using BangumiSubReNamer.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace BangumiSubReNamer.Views.Pages
{
    public partial class RenamerPage : INavigableView<RenamerViewModel>
    {
        public RenamerViewModel ViewModel { get; }

        public RenamerPage(RenamerViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            Style itemContainerStyleSub = new Style(typeof(ListViewItem));
            itemContainerStyleSub.Setters.Add(new Setter(ListViewItem.AllowDropProperty, true));
            itemContainerStyleSub.Setters.Add(new EventSetter(ListViewItem.PreviewMouseLeftButtonDownEvent,
                new MouseButtonEventHandler(OnPreviewMouseLeftButtonDown)));
            itemContainerStyleSub.Setters.Add(new EventSetter(ListViewItem.DropEvent,
                new DragEventHandler(OnSubListItemDrag)));
            UI_SubFileList.ItemContainerStyle = itemContainerStyleSub;
            
            Style itemContainerStyleSource = new Style(typeof(ListViewItem));
            itemContainerStyleSource.Setters.Add(new Setter(ListViewItem.AllowDropProperty, true));
            itemContainerStyleSource.Setters.Add(new EventSetter(ListViewItem.PreviewMouseLeftButtonDownEvent,
                new MouseButtonEventHandler(OnPreviewMouseLeftButtonDown)));
            itemContainerStyleSource.Setters.Add(new EventSetter(ListViewItem.DropEvent,
                new DragEventHandler(OnSourceListItemDrag)));
            UI_SourceFileList.ItemContainerStyle = itemContainerStyleSource;

            InitUI();
        }

        private void InitUI()
        {
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
                ViewModel.SourceFilePaths.Insert(targetIdx + 1, droppedDataFilePath);
                ViewModel.SourceFilePaths.RemoveAt(removedIdx);
            }
            else
            {
                int remIdx = removedIdx + 1;
                if (ViewModel.SourceFilePaths.Count + 1 > remIdx)
                {
                    ViewModel.SourceFilePaths.Insert(targetIdx, droppedDataFilePath);
                    ViewModel.SourceFilePaths.RemoveAt(remIdx);
                }
            }
        }

        private void OnSubListItemDrag(object sender, DragEventArgs e)
        {
            DataFilePath? droppedDataFilePath = e.Data.GetData(typeof(DataFilePath)) as DataFilePath;
            DataFilePath? target = ((ListBoxItem)(sender)).DataContext as DataFilePath;

            if (droppedDataFilePath == null || target == null) return;

            int removedIdx = UI_SubFileList.Items.IndexOf(droppedDataFilePath);
            int targetIdx = UI_SubFileList.Items.IndexOf(target);

            if (removedIdx < targetIdx)
            {
                ViewModel.ShowSubFilePaths.Insert(targetIdx + 1, droppedDataFilePath);
                ViewModel.ShowSubFilePaths.RemoveAt(removedIdx);
            }
            else
            {
                int remIdx = removedIdx + 1;
                if (ViewModel.ShowSubFilePaths.Count + 1 > remIdx)
                {
                    ViewModel.ShowSubFilePaths.Insert(targetIdx, droppedDataFilePath);
                    ViewModel.ShowSubFilePaths.RemoveAt(remIdx);
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