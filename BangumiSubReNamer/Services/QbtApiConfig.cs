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

    public async Task<bool> QbtApi_AddFeed(string feedUrl, string feedName)
    {
        var api = new Uri(new Uri(GlobalConfig.Instance.QbtWebUrl), "/api/v2/rss/addFeed");
        var param = new Dictionary<string, string>();
        param.Add("url", feedUrl);
        param.Add("path", feedName);

        try
        {
            var response = await qbtApiClient.PostAsync(api, new FormUrlEncodedContent(param));
            Console.WriteLine("qBittorrent Add Feed " + response.StatusCode);
            if (response.StatusCode == System.Net.HttpStatusCode.OK) return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return false;
    }

    public async Task<bool> QbtApi_AddRule(DataAddRssRule addRssRuleData, string ruleName)
    {
        var api = new Uri(new Uri(GlobalConfig.Instance.QbtWebUrl), "/api/v2/rss/setRule");
        try
        {
            var param = new Dictionary<string, string>();
            param.Add("ruleName", ruleName);
            param.Add("ruleDef", JsonSerializer.Serialize(addRssRuleData));

            var response = await qbtApiClient.PostAsync(api, new FormUrlEncodedContent(param));
            Console.WriteLine("qBittorrent Set Rule " + response.StatusCode);
            if (response.StatusCode == System.Net.HttpStatusCode.OK) return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return false;
    }
}