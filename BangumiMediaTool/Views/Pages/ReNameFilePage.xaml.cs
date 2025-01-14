using BangumiMediaTool.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace BangumiMediaTool.Views.Pages;

public partial class ReNameFilePage : INavigableView<ReNameFileViewModel>
{
    public ReNameFileViewModel ViewModel { get; private set; }

    public ReNameFilePage(ReNameFileViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}