using System.Windows.Controls;
using System.Windows.Input;
using BangumiMediaTool.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace BangumiMediaTool.Views.Pages;

public partial class SearchDataPage : INavigableView<SearchDataViewModel>
{
    public SearchDataViewModel ViewModel { get; private set; }

    public SearchDataPage(SearchDataViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
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