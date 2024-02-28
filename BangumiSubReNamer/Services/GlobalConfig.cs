using BangumiSubReNamer.Models;

namespace BangumiSubReNamer.Services;

public class GlobalConfig
{
    public static GlobalConfig Instance; 
    
    public GlobalConfig()
    {
        Instance = this;
        
        Console.WriteLine("config instance");
        
        ReNamerConfig = new DataReNamerConfig(
            subFileExtensions: ".ass|.srt",
            sourceFileExtensions: ".mp4|.mkv|.flv",
            defaultAddExtensions: "|.chs|.cht",
            subFileExtensionRegex: @"(\[|\]|\(|\))");
    }

    public DataReNamerConfig ReNamerConfig;
}