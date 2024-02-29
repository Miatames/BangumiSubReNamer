using System.Configuration;
using BangumiSubReNamer.Models;

namespace BangumiSubReNamer.Services;

public class GlobalConfig
{
    public static GlobalConfig Instance;

    public GlobalConfig()
    {
        Instance = this;

        Console.WriteLine("config instance");

        ReadConfig();
    }

    public DataReNamerConfig ReNamerConfig;

    public void ReadConfig()
    {
        var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        ReNamerConfig = new DataReNamerConfig(
            subFileExtensions: configuration.AppSettings.Settings["字幕匹配"].Value,
            sourceFileExtensions: configuration.AppSettings.Settings["视频匹配"].Value,
            defaultAddExtensions: configuration.AppSettings.Settings["默认扩展名"].Value,
            subFileExtensionRegex: configuration.AppSettings.Settings["字幕排除"].Value);
    }

    public void WriteConfig(DataReNamerConfig dataReNamerConfig)
    {
        var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        configuration.AppSettings.Settings["字幕匹配"].Value = dataReNamerConfig.SubFileExtensions;
        configuration.AppSettings.Settings["视频匹配"].Value = dataReNamerConfig.SourceFileExtensions;
        configuration.AppSettings.Settings["默认扩展名"].Value = dataReNamerConfig.DefaultAddExtensions;
        configuration.AppSettings.Settings["字幕排除"].Value = dataReNamerConfig.SubFileExtensionRegex;
        
        configuration.Save();
        ConfigurationManager.RefreshSection("appSettings");
    }
}