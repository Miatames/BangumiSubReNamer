using System.Net.Http;
using System.Text.Json;
using BangumiSubReNamer.Models;

namespace BangumiSubReNamer.Services;

public class QbtApiConfig
{
    public static QbtApiConfig Instance { get; private set; }

    private readonly HttpClient qbtApiClient;

    public QbtApiConfig()
    {
        Instance = this;

        qbtApiClient = new HttpClient();
    }

    public async Task QbtApi_AddFeed(string feedUrl, string feedName)
    {
        var api = GlobalConfig.Instance.QbtWebUrl + "/api/v2/rss/addFeed";
        var param = new Dictionary<string, string>();
        param.Add("url", feedUrl);
        param.Add("path", feedName);

        try
        {
            var response = await qbtApiClient.PostAsync(api, new FormUrlEncodedContent(param));
            Console.WriteLine("qBittorrent Add Feed" + response.StatusCode);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async Task QbtApi_AddRule(DataAddRssRule addRssRuleData, string ruleName)
    {
        var api = GlobalConfig.Instance.QbtWebUrl + "/api/v2/rss/setRule";
        try
        {
            var param = new Dictionary<string, string>();
            param.Add("ruleName", ruleName);
            param.Add("ruleDef",JsonSerializer.Serialize(addRssRuleData));

            var response = await qbtApiClient.PostAsync(api, new FormUrlEncodedContent(param));
            Console.WriteLine("qBittorrent Set Rule" + response.StatusCode);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}