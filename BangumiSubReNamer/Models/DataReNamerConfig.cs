namespace BangumiSubReNamer.Models;

public class DataReNamerConfig
{
     public DataReNamerConfig(string sourceFileExtensions, string subFileExtensions, string defaultAddExtensions, string subFileExtensionRegex)
     {
          SourceFileExtensions = sourceFileExtensions;
          SubFileExtensions = subFileExtensions;
          DefaultAddExtensions = defaultAddExtensions;
          SubFileExtensionRegex = subFileExtensionRegex;
     }

     /// <summary>
     /// 可拖入的扩展名
     /// </summary>
     public string SourceFileExtensions { get; set;}

     /// <summary>
     /// 可拖入的扩展名
     /// </summary>
     public string SubFileExtensions { get; set; }
     
     /// <summary>
     /// 默认插入的扩展名
     /// </summary>
     public string DefaultAddExtensions { get; set;}

     public string SubFileExtensionRegex { get; set;}

}