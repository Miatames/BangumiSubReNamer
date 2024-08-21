using System.Windows.Controls;
using BangumiSubReNamer.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace BangumiSubReNamer.Views.Pages;

public partial class QbtRssPage : INavigableView<QbtRssViewModel>
{
    public QbtRssViewModel ViewModel { get; }

    public QbtRssPage(QbtRssViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}