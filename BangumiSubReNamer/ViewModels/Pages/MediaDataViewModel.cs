using System.Collections.ObjectModel;
using System.Text.Json;
using BangumiSubReNamer.Models;
using BangumiSubReNamer.Services;
using CommunityToolkit.Mvvm.Messaging;
using Wpf.Ui.Controls;

namespace BangumiSubReNamer.ViewModels.Pages
{
    public partial class MediaDataViewModel : ObservableRecipient, INavigationAware,
        IRecipient<DataWindowSize>, IRecipient<DataSearchStrMessage>
    {
        public MediaDataViewModel()
        {
            WeakReferenceMessenger.Default.Register<DataWindowSize>(this);
            WeakReferenceMessenger.Default.Register<DataSearchStrMessage>(this);

            Console.WriteLine("init MediaDataPageViewModel");
        }

        [ObservableProperty] private int height = 580;
        [ObservableProperty] private Visibility isProcess = Visibility.Hidden;
        [ObservableProperty] private string searchText = string.Empty;

        [ObservableProperty] private DataSubjectsInfo? searchListSelectItem;
        [ObservableProperty] private ObservableCollection<DataSubjectsInfo> dataSearchResultsList = new();
        [ObservableProperty] private Visibility isSearch = Visibility.Visible;
        [ObservableProperty] private bool isSearchAllResults = false;

        [ObservableProperty] private ObservableCollection<DataEpisodesInfo> dataEpisodesInfosList = new();
        [ObservableProperty] private ObservableCollection<DataEpisodesInfo> episodesInfoSelectItems = new();
        [ObservableProperty] private Visibility isEpisodes = Visibility.Hidden;

        public void Receive(DataWindowSize message)
        {
            Height = message.Height - 70;
        }

        public void Receive(DataSearchStrMessage message)
        {
            SearchText = message.SearchStr;
        }

        [RelayCommand]
        private async void OnSearch()
        {
            await DoSearchWithText(SearchText);
        }

        [RelayCommand]
        private void OnSearchBack()
        {
            IsSearch = Visibility.Visible;
            IsEpisodes = Visibility.Hidden;
            SearchListSelectItem = null;
        }

        public void OnNavigatedTo()
        {
            // IsProcess = Visibility.Hidden;
            Height = GlobalConfig.Instance.Height - 70;
        }

        public void OnNavigatedFrom() { }

        private async Task DoSearchWithText(string keywords)
        {
            DataSearchResultsList.Clear();
            SearchListSelectItem = null;

            IsProcess = Visibility.Visible;
            IsSearch = Visibility.Hidden;
            IsEpisodes = Visibility.Hidden;
            var results = await BangumiApiConfig.Instance.BangumiApi_Search(keywords, IsSearchAllResults);
            IsProcess = Visibility.Hidden;
            IsSearch = Visibility.Visible;

            try
            {
                foreach (var result in results)
                {
                    var json = JsonSerializer.Deserialize<BgmApiJson_Search>(result);
                    if (json == null) continue;
                    foreach (var listItem in json.list)
                    {
                        DataSearchResultsList.Add(new DataSubjectsInfo(
                            listItem.id, listItem.name, listItem.name_cn, listItem.eps_count, listItem.air_date));
                    }
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
            IsSearch = Visibility.Hidden;
            IsEpisodes = Visibility.Hidden;
            var result = await BangumiApiConfig.Instance.BangumiApi_Episodes(id);
            var resultSp = await BangumiApiConfig.Instance.BangumiApi_EpisodesSp(id);
            IsProcess = Visibility.Hidden;
            IsEpisodes = Visibility.Visible;

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
        }

        partial void OnSearchListSelectItemChanged(DataSubjectsInfo? value)
        {
            if (value != null)
            {
                Console.WriteLine(value.ShowText);
                DoGetEpisodesInfoWithId(value.Name,value.NameCn, value.Id.ToString(), value.AirDate);
            }
            else
            {
                Console.WriteLine("select no item");
            }
        }
    }
}