using System.Configuration;
using System.IO;
using BangumiSubReNamer.Models;
using FFMpegCore;

namespace BangumiSubReNamer.Services;

public class GlobalConfig
{
    public static GlobalConfig Instance;

    public GlobalConfig()
    {
        Instance = this;
        ReNamerConfig = new DataReNamerConfig("", "", "", "");

        Console.WriteLine("config instance");

        ReadConfig();
    }

    public DataReNamerConfig ReNamerConfig;
    public string OutFilePath;
    public string CreateFileNameTemplateBangumi;
    public string CreateFileNameTemplateMovie;
    public string QbtWebUrl;
    public string QbtDownloadPath;
    public string FFmpegBinariesPath;

    public void ReadConfig()
    {
        var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        try
        {
            ReNamerConfig = new DataReNamerConfig(
                addSubFileExtensionRegex: configuration.AppSettings.Settings["字幕匹配"].Value,
                addSourceFileExtensionRegex: configuration.AppSettings.Settings["视频匹配"].Value,
                defaultAddExtensions: configuration.AppSettings.Settings["默认扩展名"].Value,
                subFileExtensionRegex: configuration.AppSettings.Settings["字幕排除"].Value);

            OutFilePath = configuration.AppSettings.Settings["硬链接默认路径"].Value;
            CreateFileNameTemplateBangumi = configuration.AppSettings.Settings["剧集文件名模板"].Value;
            CreateFileNameTemplateMovie = configuration.AppSettings.Settings["电影文件名模板"].Value;
            QbtWebUrl = configuration.AppSettings.Settings["qBittorrent网页地址"].Value;
            QbtDownloadPath = configuration.AppSettings.Settings["qBittorrent下载路径"].Value;
            FFmpegBinariesPath = configuration.AppSettings.Settings["FFmpeg文件夹路径"].Value;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        // GlobalFFOptions.Configure(new FFOptions(){BinaryFolder = FFmpegBinariesPath});
    }

    public void WriteConfig()
    {
        var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        try
        {
            configuration.AppSettings.Settings["字幕匹配"].Value = ReNamerConfig.AddSubFileExtensionRegex;
            configuration.AppSettings.Settings["视频匹配"].Value = ReNamerConfig.AddSourceFileExtensionRegex;
            configuration.AppSettings.Settings["默认扩展名"].Value = ReNamerConfig.DefaultAddExtensions;
            configuration.AppSettings.Settings["字幕排除"].Value = ReNamerConfig.SubFileExtensionRegex;
            configuration.AppSettings.Settings["硬链接默认路径"].Value = OutFilePath;
            configuration.AppSettings.Settings["剧集文件名模板"].Value = CreateFileNameTemplateBangumi;
            configuration.AppSettings.Settings["电影文件名模板"].Value = CreateFileNameTemplateMovie;
            configuration.AppSettings.Settings["qBittorrent网页地址"].Value = QbtWebUrl;
            configuration.AppSettings.Settings["qBittorrent下载路径"].Value = QbtDownloadPath;
            configuration.AppSettings.Settings["ffmpeg文件夹路径"].Value = FFmpegBinariesPath;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        configuration.Save();
        ConfigurationManager.RefreshSection("appSettings");
    }
}