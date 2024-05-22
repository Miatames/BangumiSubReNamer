using System.Net;
using System.Net.Http;
using System.Text.Json;
using BangumiSubReNamer.Models;
using Fluid;

namespace BangumiSubReNamer.Services;

public class BangumiApiConfig
{
    private readonly string apiUrl = @"https://api.bgm.tv";

    public static BangumiApiConfig Instance;

    private readonly HttpClient bgmApiClient;
    private readonly HttpClient tmdbApiClient;
    private readonly FluidParser fileNameParser;

    public BangumiApiConfig()
    {
        Instance = this;

        bgmApiClient = new HttpClient();
        bgmApiClient.DefaultRequestHeaders.Add("Accept", "application/json");
        bgmApiClient.DefaultRequestHeaders.Add("User-Agent",
            "BangumiSubReNamer/0.4 (https://github.com/Miatames/BangumiSubReNamer)");

        tmdbApiClient = new HttpClient();
        tmdbApiClient.DefaultRequestHeaders.Add("Accept", "application/json");
        tmdbApiClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + TmdbInfo.Authorization);

        fileNameParser = new FluidParser();
    }

    public async Task<string> TmdbApi_Search(string keywords)
    {
        var resultTitle = "";
        if (string.IsNullOrEmpty(keywords)) return resultTitle;

        var url = $"http://api.themoviedb.org/3/search/multi?query={Uri.EscapeDataString(keywords)}&include_adult=false&page=1";

        try
        {
            var response = await tmdbApiClient.GetAsync(url);
            Console.WriteLine("request: " + url + " : " + response.StatusCode);
            if (!response.IsSuccessStatusCode) return resultTitle;

            var jsonRoot = JsonSerializer.Deserialize<TmdbApiJson_Search>(await response.Content.ReadAsStringAsync());
            if (jsonRoot is { results.Count: > 0 })
            {
                foreach (var resultsItem in jsonRoot.results)
                {
                    if (resultsItem.media_type == "tv")
                    {
                        resultTitle = resultsItem.original_name;
                        break;
                    }
                    else if (resultsItem.media_type == "movie")
                    {
                        resultTitle = resultsItem.original_title;
                        break;
                    }
                }
            }

            return resultTitle;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return resultTitle;
        }
    }

    public async Task<List<string>> BangumiApi_Search(string keywords, bool getAllResults = false)
    {
        var results = new List<string>();
        if (string.IsNullOrEmpty(keywords)) return results;

        var url = $"{apiUrl}/search/subject/{Uri.EscapeDataString(keywords)}?type=2&responseGroup=large&start=0&max_results=25";
        try
        {
            var response = await bgmApiClient.GetAsync(url);
            Console.WriteLine("request: " + url + " : " + response.StatusCode);
            if (!response.IsSuccessStatusCode) return results;

            var result = await response.Content.ReadAsStringAsync();

            if (result.StartsWith("<"))
            {
                return results;
            }

            using var document = JsonDocument.Parse(result);
            var root = document.RootElement;

            //请求出错直接返回
            if (root.TryGetProperty("error", out _))
            {
                return results;
            }

            results.Add(result);

            if (!getAllResults) return results;

            if (root.TryGetProperty("results", out JsonElement maxCount) && maxCount.GetInt32() >= 25)
            {
                for (int i = 1; i < Math.Ceiling(maxCount.GetSingle() / 25.0f); i++)
                {
                    var urlLoop =
                        $"{apiUrl}/search/subject/{Uri.EscapeDataString(keywords)}?type=2&responseGroup=large&start={i * 25}&max_results=25";
                    Console.WriteLine("request: " + urlLoop);
                    var responseLoop = await bgmApiClient.GetAsync(urlLoop);
                    if (!responseLoop.IsSuccessStatusCode) continue;

                    var resultLoop = await responseLoop.Content.ReadAsStringAsync();
                    results.Add(resultLoop);
                }
            }

            return results;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return results;
        }
    }

    public async Task<string> BangumiApi_Episodes(string subject_id)
    {
        if (string.IsNullOrEmpty(subject_id)) return "";

        var url = $"{apiUrl}/v0/episodes?subject_id={subject_id}&type=0";

        try
        {
            var response = await bgmApiClient.GetAsync(url);
            Console.WriteLine("request: " + url + " : " + response.StatusCode);
            if (!response.IsSuccessStatusCode) return "";

            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "";
        }
    }

    public async Task<string> BangumiApi_EpisodesSp(string subject_id)
    {
        if (string.IsNullOrEmpty(subject_id)) return "";

        var url = $"{apiUrl}/v0/episodes?subject_id={subject_id}&type=1";

        try
        {
            var response = await bgmApiClient.GetAsync(url);
            Console.WriteLine("request: " + url + " : " + response.StatusCode);
            if (!response.IsSuccessStatusCode) return "";

            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "";
        }
    }

    public string BangumiNewFileName(DataEpisodesInfo info, string sourceFileName, int padLeft)
    {
        var fileName = sourceFileName;
        var templateFileName = GlobalConfig.Instance.CreateFileNameTemplateBangumi;

        var data = new
        {
            SubjectId = info.SubjectId,
            SubjectName = info.SubjectName,
            SubjectNameCn = info.SubjectNameCn,
            EpisodeId = info.Id,
            EpisodeName = info.Name,
            EpisodeNameCn = info.NameCn,
            EpisodesSort = (info.Type == 0 ? "S1E" : "S0E") + info.Sort.ToString().PadLeft(padLeft, '0'),
            Year = info.Year,
            SourceFileName = sourceFileName
        };

        if (fileNameParser.TryParse(templateFileName, out var template))
        {
            var context = new TemplateContext(data);

            fileName = template.Render(context);
        }

        return fileName;
    }

    public string MovieNewFileName(DataEpisodesInfo info, string sourceFileName)
    {
        var fileName = sourceFileName;
        var templateFileName = GlobalConfig.Instance.CreateFileNameTemplateMovie;

        var data = new
        {
            SubjectId = info.SubjectId,
            SubjectName = info.SubjectName,
            SubjectNameCn = info.SubjectNameCn,
            EpisodeId = info.Id,
            EpisodeName = info.Name,
            EpisodeNameCn = info.NameCn,
            Year = info.Year,
            SourceFileName = sourceFileName
        };

        if (fileNameParser.TryParse(templateFileName, out var template))
        {
            var context = new TemplateContext(data);

            fileName = template.Render(context);
        }

        return fileName;
    }
}