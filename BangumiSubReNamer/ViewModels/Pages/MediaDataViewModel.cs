using System.Collections.ObjectModel;
using System.Text.Json;
using BangumiSubReNamer.Models;
using BangumiSubReNamer.Services;
using CommunityToolkit.Mvvm.Messaging;
using Wpf.Ui.Controls;
using ListView = Wpf.Ui.Controls.ListView;

namespace BangumiSubReNamer.ViewModels.Pages
{
    public partial class MediaDataViewModel : ObservableRecipient, INavigationAware, IRecipient<DataSearchStrMessage>
    {
        public MediaDataViewModel()
        {
            WeakReferenceMessenger.Default.Register<DataSearchStrMessage>(this);

            Console.WriteLine("init MediaDataPageViewModel");
        }

        [ObservableProperty] private Visibility isProcess = Visibility.Hidden;
        [ObservableProperty] private string searchText = string.Empty;

        [ObservableProperty] private DataSubjectsInfo? searchListSelectItem;
        [ObservableProperty] private ObservableCollection<DataSubjectsInfo> dataSearchResultsList = new();
        [ObservableProperty] private bool isSearchAllResults = false;

        [ObservableProperty] private ObservableCollection<DataEpisodesInfo> dataEpisodesInfosList = new();
        [ObservableProperty] private ObservableCollection<DataEpisodesInfo> episodesInfoSelectItems = new();
        [ObservableProperty] private Visibility isEpisodes = Visibility.Hidden;

        [ObservableProperty] private string subjectInfo_Name = "";
        [ObservableProperty] private string subjectInfo_Count = "";
        [ObservableProperty] private string subjectInfo_Date = "";
        [ObservableProperty] private string subjectInfo_Url = "";
        [ObservableProperty] private string subjectInfo_Desc = "";

        private Dictionary<string, string> resultsDict = new Dictionary<string, string>();
        private Dictionary<string, string> resultsSpDict = new Dictionary<string, string>();

        public void Receive(DataSearchStrMessage message)
        {
            SearchText = message.SearchStr;
        }

        [RelayCommand]
        private async Task OnSearch()
        {
            await DoSearchWithText(SearchText);
        }

        [RelayCommand]
        private async Task OnGetEpisodesInfo()
        {
            await DoGetEpisodesInfoWithId(SearchListSelectItem.Name, SearchListSelectItem.NameCn,
                SearchListSelectItem.Id.ToString(), SearchListSelectItem.AirDate);
        }

        [RelayCommand]
        private async Task OnAddToRename(object sender)
        {
            if (sender is not ListView listView) return;

            await DoAddEpoisodesToRename(listView);
        }

        public void OnNavigatedTo()
        {
            // IsProcess = Visibility.Hidden;
            // Height = GlobalConfig.Instance.Height - 70;
        }

        public void OnNavigatedFrom() { }

        private async Task DoAddEpoisodesToRename(ListView listView)
        {
            var selectedItems = listView.SelectedItems.Cast<DataEpisodesInfo>().ToList();

            var infoList = new DataEpisodesInfoList();

            if (DataEpisodesInfosList.Count == 0)
            {
                await DoGetEpisodesInfoWithId(SearchListSelectItem.Name, SearchListSelectItem.NameCn,
                    SearchListSelectItem.Id.ToString(), SearchListSelectItem.AirDate);
            }

            Console.WriteLine("item count: " + DataEpisodesInfosList.Count);

            if (selectedItems.Count == 0)
            {
                infoList = new DataEpisodesInfoList() { Infos = DataEpisodesInfosList.ToList() };
            }
            else
            {
                foreach (var info in selectedItems)
                {
                    infoList.Infos.Add(info);
                }
            }

            WeakReferenceMessenger.Default.Send<DataEpisodesInfoList>(infoList);
            WeakReferenceMessenger.Default.Send<DataSnackbarMessage>(
                new DataSnackbarMessage("添加到元数据", "", ControlAppearance.Success));
        }

        private async Task DoSearchWithText(string keywords)
        {
            DataSearchResultsList.Clear();
            SearchListSelectItem = null;

            IsProcess = Visibility.Visible;
            IsEpisodes = Visibility.Hidden;
            var results = await BangumiApiConfig.Instance.BangumiApi_Search(keywords, IsSearchAllResults);
            IsProcess = Visibility.Hidden;

            try
            {
                foreach (var result in results)
                {
                    var json = JsonSerializer.Deserialize<BgmApiJson_Search>(result);
                    if (json == null) continue;
                    foreach (var listItem in json.list)
                    {
                        DataSearchResultsList.Add(new DataSubjectsInfo(
                            listItem.id, listItem.name, listItem.name_cn, listItem.eps_count, listItem.summary,
                            listItem.air_date));
                    }
                }

                if (DataSearchResultsList.Count > 0)
                {
                    IsEpisodes = Visibility.Visible;
                    SearchListSelectItem = DataSearchResultsList[0];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task DoGetEpisodesInfoWithId(string subjectName, string subjectNameCn, string id, string airDate)
        {
            DataEpisodesInfosList.Clear();
            EpisodesInfoSelectItems.Clear();

            IsProcess = Visibility.Visible;
            var result = "";
            var resultSp = "";

            if (resultsDict.ContainsKey(id) && resultsDict[id] != "")
            {
                result = resultsDict[id];
            }
            else
            {
                result = await BangumiApiConfig.Instance.BangumiApi_Episodes(id);
                resultsDict.Add(id, result);
            }

            if (resultsSpDict.ContainsKey(id) && resultsSpDict[id] != "")
            {
                resultSp = resultsSpDict[id];
            }
            else
            {
                resultSp = await BangumiApiConfig.Instance.BangumiApi_EpisodesSp(id);
                resultsSpDict.Add(id, resultSp);
            }

            IsProcess = Visibility.Hidden;

            if (result == "" || resultSp == "")
            {
                Console.WriteLine($"null json");
                return;
            }

            try
            {
                var json = JsonSerializer.Deserialize<BgmApiJson_EpisodesInfo>(result);
                if (json == null) return;
                foreach (var listItem in json.data)
                {
                    DataEpisodesInfosList.Add(new DataEpisodesInfo(listItem.id, listItem.name, listItem.name_cn,
                        subjectName, subjectNameCn, listItem.ep, listItem.sort, listItem.subject_id,
                        0, DateTime.Parse(airDate).Year.ToString()));
                }

                var jsonSp = JsonSerializer.Deserialize<BgmApiJson_EpisodesInfo>(resultSp);
                if (jsonSp == null) return;
                foreach (var listItem in jsonSp.data)
                {
                    DataEpisodesInfosList.Add(new DataEpisodesInfo(listItem.id, listItem.name, listItem.name_cn,
                        subjectName, subjectNameCn, listItem.ep, listItem.sort, listItem.subject_id,
                        1, DateTime.Parse(airDate).Year.ToString()));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            await Task.Delay(100);
        }

        partial void OnSearchListSelectItemChanged(DataSubjectsInfo? value)
        {
            if (value != null)
            {
                Console.WriteLine(value.ShowText);
                // DoGetEpisodesInfoWithId(value.Name, value.NameCn, value.Id.ToString(), value.AirDate);

                SubjectInfo_Name = $"{value.NameCn} ({value.Name})";
                SubjectInfo_Count = value.EpsCount.ToString();
                SubjectInfo_Date = value.AirDate;
                SubjectInfo_Desc = value.Desc;
                SubjectInfo_Url = @"https://bgm.tv/subject/" + value.Id;

                DataEpisodesInfosList.Clear();
            }
            else
            {
                Console.WriteLine("select no item");

                SubjectInfo_Name = "";
                SubjectInfo_Count = "";
                SubjectInfo_Date = "";
                SubjectInfo_Desc = "";
                SubjectInfo_Url = "";

                DataEpisodesInfosList.Clear();
            }
        }
    }
}