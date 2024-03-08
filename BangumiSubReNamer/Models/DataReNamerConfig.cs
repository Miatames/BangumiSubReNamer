namespace BangumiSubReNamer.Models;

public class DataReNamerConfig
{
     public DataReNamerConfig(string addSourceFileExtensionRegex, string addSubFileExtensionRegex, string defaultAddExtensions, string subFileExtensionRegex)
     {
          AddSourceFileExtensionRegex = addSourceFileExtensionRegex;
          AddSubFileExtensionRegex = addSubFileExtensionRegex;
          DefaultAddExtensions = defaultAddExtensions;
          SubFileExtensionRegex = subFileExtensionRegex;
     }

     /// <summary>
     /// 可拖入的扩展名
     /// </summary>
     public string AddSourceFileExtensionRegex { get; set;}

     /// <summary>
     /// 可拖入的扩展名
     /// </summary>
     public string AddSubFileExtensionRegex { get; set; }
     
     /// <summary>
     /// 默认插入的扩展名
     /// </summary>
     public string DefaultAddExtensions { get; set;}

     public string SubFileExtensionRegex { get; set;}

}