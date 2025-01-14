using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using BangumiMediaTool.Models;
using BangumiMediaTool.Services.Program;
using Fluid;

namespace BangumiMediaTool.Services.Page;

public static class CreateFileService
{
    /// <summary>
    /// 新文件夹名称
    /// </summary>
    /// <param name="info">元数据</param>
    /// <returns></returns>
    public static string NewFolderName(DataEpisodesInfo info)
    {
        var folderName = "文件夹";
        var templateFolderName = GlobalConfig.Instance.AppConfig.CreateFolderNameTemplate;

        var data = new
        {
            SubjectId = info.SubjectId,
            SubjectName = info.SubjectName,
            SubjectNameCn = info.SubjectNameCn,
            Year = info.Year
        };

        var fileNameParser = new FluidParser();
        if (fileNameParser.TryParse(templateFolderName, out var template))
        {
            var context = new TemplateContext(data);

            folderName = template.Render(context);
        }

        return folderName.RemoveInvalidFileNameChar();
    }

    /// <summary>
    /// 新剧集文件名称
    /// </summary>
    /// <param name="info">元数据</param>
    /// <param name="sourceFileName">源文件名</param>
    /// <param name="padLeft">剧集编号左侧填0的数量</param>
    /// <returns></returns>
    public static string BangumiNewFileName(DataEpisodesInfo info, string sourceFileName, int padLeft)
    {
        var fileName = sourceFileName;
        var extensionName = Path.GetExtension(fileName);
        var templateFileName = GlobalConfig.Instance.AppConfig.CreateBangumiFileNameTemplate;
        if (padLeft < 2) padLeft = 2;

        var data = new
        {
            SubjectId = info.SubjectId,
            SubjectName = info.SubjectName,
            SubjectNameCn = info.SubjectNameCn,
            EpisodeId = info.Id,
            EpisodeName = info.Name,
            EpisodeNameCn = info.NameCn,
            EpisodesSort = (info.Type == 0 ? "S01E" : "S00E") + info.Sort.ToString().PadLeft(padLeft, '0'),
            Year = info.Year,
            SourceFileName = Path.GetFileNameWithoutExtension(sourceFileName)
        };

        var fileNameParser = new FluidParser();
        if (fileNameParser.TryParse(templateFileName, out var template))
        {
            var context = new TemplateContext(data);

            fileName = template.Render(context) + extensionName;
        }

        return fileName.RemoveInvalidFileNameChar();
    }

    /// <summary>
    /// 新电影文件名称
    /// </summary>
    /// <param name="info">元数据</param>
    /// <param name="sourceFileName">源文件名</param>
    /// <returns></returns>
    public static string MovieNewFileName(DataEpisodesInfo info, string sourceFileName)
    {
        var fileName = sourceFileName;
        var extensionName = Path.GetExtension(fileName);
        var templateFileName = GlobalConfig.Instance.AppConfig.CreateMovieFileNameTemplate;

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

        var fileNameParser = new FluidParser();
        if (fileNameParser.TryParse(templateFileName, out var template))
        {
            var context = new TemplateContext(data);

            fileName = template.Render(context);

            if (!templateFileName.Contains("SourceFileName"))
            {
                fileName = fileName + extensionName;
            }
        }

        return fileName.RemoveInvalidFileNameChar();
    }

    /// <summary>
    /// 创建Nfo文件
    /// </summary>
    /// <param name="info">数据</param>
    /// <param name="filePath">文件路径</param>
    public static void CreateNfoFromData<T>(T info, string filePath)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            using var writer = new XmlTextWriter(filePath, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            var namespaces = new XmlSerializerNamespaces([new XmlQualifiedName(string.Empty, string.Empty)]);
            serializer.Serialize(writer, info, namespaces);
        }
        catch (Exception e)
        {
           Logs.LogError(e.ToString());
        }
    }
}
