using System.IO;
using BangumiSubReNamer.Models;
using BangumiSubReNamer.Services;
using CommunityToolkit.Mvvm.Messaging;
using Wpf.Ui.Controls;

namespace BangumiSubReNamer.ViewModels.Pages;

public partial class QbtRssViewModel : ObservableRecipient, INavigationAware, IRecipient<DataSubjectsInfo>
{
    public QbtRssViewModel()
    {
        WeakReferenceMessenger.Default.Register<DataSubjectsInfo>(this);

        Console.WriteLine("init QbtRssViewModel");
    }

    [ObservableProperty] private string bangumiName = "";
    [ObservableProperty] private string bangumiId = "";
    [ObservableProperty] private string rssFeedPath = "";
    [ObservableProperty] private string rssRuleName = "";
    [ObservableProperty] private bool isUseNameCn = true;
    [ObservableProperty] private bool enableRule = true;

    private DataSubjectsInfo? dataSubjectsInfo = null;

    public void Receive(DataSubjectsInfo message)
    {
        dataSubjectsInfo = message;

        BangumiName = IsUseNameCn ? dataSubjectsInfo.NameCn : dataSubjectsInfo.Name;
        BangumiId = dataSubjectsInfo.Id.ToString();
    }

    [RelayCommand]
    private async Task OnAddRssToQbt()
    {
        if (BangumiName.Length == 0 || RssFeedPath.Length == 0 || GlobalConfig.Instance.QbtDownloadPath.Length == 0) return;

        var folderPath = Path.Combine(GlobalConfig.Instance.QbtDownloadPath, BangumiName);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        if (dataSubjectsInfo != null)
        {
            var subjectsInfo = new NfoInfo_SubjectsRootTv
            {
                bangumiid = dataSubjectsInfo.Id.ToString(),
                title = dataSubjectsInfo.NameCn,
                originaltitle = dataSubjectsInfo.Name,
                showtitle = dataSubjectsInfo.NameCn,
                year = DateTime.Parse(dataSubjectsInfo.AirDate).Year.ToString(),
            };
            ExtensionTools.RunCreateNfoFile(subjectsInfo, Path.Combine(folderPath, "tvshow.nfo"));
        }

        var dataAddRssRule = new DataAddRssRule()
        {
            Enabled = EnableRule,
            MustContain = "",
            MustNotContain = "",
            UseRegex = false,
            EpisodeFilter = "",
            SmartFilter = false,
            PreviouslyMatchedEpisodes = [],
            AffectedFeeds = [RssFeedPath],
            IgnoreDays = 0,
            LastMatch = "",
            AddPaused = false,
            AssignedCategory = "Bangumi",
            SavePath = Path.Combine(folderPath, "Season 1")
        };

        var addRuleName = RssRuleName.Length == 0 ? BangumiName : RssRuleName;

        bool addFeedSuccess = await QbtApiConfig.Instance.QbtApi_AddFeed(RssFeedPath, addRuleName);
        bool addRuleSuccess = await QbtApiConfig.Instance.QbtApi_AddRule(dataAddRssRule, addRuleName);

        if (addFeedSuccess && addRuleSuccess)
        {
            WeakReferenceMessenger.Default.Send<DataSnackbarMessage>(
                new DataSnackbarMessage("添加Rss：",
                    $"{addRuleName} [{RssFeedPath}]",
                    ControlAppearance.Success));
        }
        else
        {
            WeakReferenceMessenger.Default.Send(new DataSnackbarMessage("添加Rss失败：",
                $"添加订阅：{addFeedSuccess}  添加下载规则：{addRuleSuccess}",
                ControlAppearance.Caution));
        }
    }

    [RelayCommand]
    private void OnSetIsUseNameCn()
    {
        if (dataSubjectsInfo != null) BangumiName = IsUseNameCn ? dataSubjectsInfo.NameCn : dataSubjectsInfo.Name;
    }

    public void OnNavigatedTo() { }

    public void OnNavigatedFrom() { }
}