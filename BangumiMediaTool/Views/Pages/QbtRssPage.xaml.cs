using Wpf.Ui.Controls;
using QbtRssViewModel = BangumiMediaTool.ViewModels.Pages.QbtRssViewModel;

namespace BangumiMediaTool.Views.Pages;

public partial class QbtRssPage : INavigableView<QbtRssViewModel>
{
    public QbtRssViewModel ViewModel { get; private set;}

    public QbtRssPage(QbtRssViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}