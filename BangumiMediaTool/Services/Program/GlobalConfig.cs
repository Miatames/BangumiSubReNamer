using System.IO;
using System.Text.Json;
using BangumiMediaTool.Models;
using FFMpegCore;

namespace BangumiMediaTool.Services.Program;

public class GlobalConfig
{
    public static GlobalConfig Instance { get; private set; } = null!;
    public AppConfig AppConfig { get; set; } = new AppConfig();

    public GlobalConfig()
    {
        Instance = this;
        ReadConfig();

        Logs.LogInfo("GlobalConfig Initialize");
    }

    public void ReadConfig()
    {
        var jsonString = File.ReadAllText("config.json");
        AppConfig = JsonSerializer.Deserialize<AppConfig>(jsonString) ?? new AppConfig();
        Logs.LogInfo(jsonString);
        GlobalFFOptions.Configure(new FFOptions() { BinaryFolder = AppConfig.FFmpegPath });
    }

    private readonly JsonSerializerOptions defaultWriteOptions = new() { WriteIndented = true };

    public void WriteConfig(AppConfig setConfig)
    {
        AppConfig = setConfig;
        var jsonString = JsonSerializer.Serialize(AppConfig, defaultWriteOptions);
        File.WriteAllText("config.json", jsonString);
        GlobalFFOptions.Configure(new FFOptions() { BinaryFolder = AppConfig.FFmpegPath });
    }
}