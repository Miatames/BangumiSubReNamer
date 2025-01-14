using BangumiMediaTool.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace BangumiMediaTool.Views.Pages;

public partial class MediaNfoDataPage : INavigableView<MediaNfoDataViewModel>
{
    public MediaNfoDataViewModel ViewModel { get; private set; }

    public MediaNfoDataPage(MediaNfoDataViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}