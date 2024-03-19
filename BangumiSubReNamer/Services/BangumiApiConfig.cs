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

    private readonly HttpClient client;
    private readonly FluidParser fileNameParser;

    public BangumiApiConfig()
    {
        Instance = this;

        client = new HttpClient();
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("User-Agent",
            "BangumiSubReNamer/0.4 (https://github.com/Miatames/BangumiSubReNamer)");

        fileNameParser = new FluidParser();
    }

    public async Task<List<string>> BangumiApi_Search(string keywords, bool getAllResults = false)
    {
        var results = new List<string>();
        if (keywords == "") return results;

        var url = $"{apiUrl}/search/subject/{WebUtility.UrlEncode(keywords)}?type=2&responseGroup=large&start=0&max_results=25";
        Console.WriteLine("request: " + url);
        try
        {
            var result = await client.GetStringAsync(url);

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
                        $"{apiUrl}/search/subject/{WebUtility.UrlEncode(keywords)}?type=2&responseGroup=large&start={i * 25}&max_results=25";
                    Console.WriteLine("request: " + urlLoop);

                    var resultLoop = await client.GetStringAsync(urlLoop);
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
        if (subject_id == "") return "";

        var url = $"{apiUrl}/v0/episodes?subject_id={subject_id}&type=0";
        Console.WriteLine("request: " + url);

        try
        {
            var result = await client.GetStringAsync(url);
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
        if (subject_id == "") return "";

        var url = $"{apiUrl}/v0/episodes?subject_id={subject_id}&type=1";
        Console.WriteLine("request: " + url);

        try
        {
            var result = await client.GetStringAsync(url);
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
            EpisodesNameCn = info.NameCn,
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
            EpisodesNameCn = info.NameCn,
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