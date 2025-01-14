using System.IO;
using System.Text.RegularExpressions;
using BangumiMediaTool.Models;
using BangumiMediaTool.Services.Program;
using BangumiMediaTool.ViewModels.Windows;
using FFMpegCore;

namespace BangumiMediaTool.Services.Page;

public static partial class ReNameFileService
{
    /// <summary>
    /// 获取匹配的字幕文件
    /// </summary>
    /// <param name="files"> 源文件列表</param>
    /// <param name="currentIndex">当前选中的扩展名序号</param>
    /// <param name="extensions">扩展名列表</param>
    /// <returns></returns>
    public static List<DataFilePath> GetMatchSubtitleFiles(this List<DataFilePath> files, int currentIndex, List<string> extensions)
    {
        List<DataFilePath> matchedFiles = [];

        if (currentIndex < 0 || currentIndex >= files.Count) return matchedFiles;
        var currentExtension = extensions[currentIndex];
        foreach (var file in files)
        {
            if (!file.FileName.EndsWith(currentExtension)) continue;
            bool isAdd = true;

            foreach (var extension in extensions
                         .Where(extension => currentExtension != extension
                                             && file.FileName.EndsWith(extension)
                                             && extension.Contains(currentExtension)))
            {
                isAdd = false;
            }

            if (isAdd) matchedFiles.Add(file);
        }

        return matchedFiles;
    }

    /// <summary>
    /// 创建新文件列表
    /// </summary>
    /// <param name="mediaFiles">媒体文件</param>
    /// <param name="subtitleFiles">字幕文件</param>
    /// <param name="selectExtension">筛选的扩展名,不筛选时为空</param>
    /// <param name="addExtension">添加的扩展名</param>
    /// <param name="fileOperateMode">文件操作模式 0:转换为SRT 1:复制 2:重命名</param>
    /// <returns></returns>
    public static List<DataFilePath> CreateNewFilePaths(List<DataFilePath> mediaFiles, List<DataFilePath> subtitleFiles,
        string selectExtension, string addExtension, int fileOperateMode)
    {
        var newFiles = new List<DataFilePath>();

        if (mediaFiles.Count == 0 && subtitleFiles.Count > 0 && fileOperateMode == 0)
        {
            foreach (var file in subtitleFiles)
            {
                var folder = Path.GetDirectoryName(file.FilePath);
                if (string.IsNullOrEmpty(folder)) continue;

                if (string.IsNullOrEmpty(selectExtension))
                {
                    selectExtension = file.FileName.GetExtensionName(GlobalConfig.Instance.AppConfig.RegexMatchSubtitleFiles);
                }

                var fileNameWithoutExtension = file.FilePath.Replace(selectExtension, string.Empty);
                newFiles.Add(new DataFilePath(fileNameWithoutExtension + addExtension + ".srt"));
            }

            return newFiles;
        }


        for (int i = 0; i < Math.Min(mediaFiles.Count, subtitleFiles.Count); i++)
        {
            var mediaFile = mediaFiles[i];
            var subtitleFile = subtitleFiles[i];
            switch (fileOperateMode)
            {
                case 0:
                    var folder0 = Path.GetDirectoryName(mediaFile.FilePath) ?? string.Empty;
                    var fileName0 = Path.GetFileNameWithoutExtension(mediaFile.FileName) + addExtension + ".srt";
                    newFiles.Add(new DataFilePath(Path.Combine(folder0, fileName0)));
                    break;
                case 1:
                    var folder1 = Path.GetDirectoryName(mediaFile.FilePath) ?? string.Empty;
                    var subExtension1 = Path.GetExtension(subtitleFile.FileName);
                    var fileName1 = Path.GetFileNameWithoutExtension(mediaFile.FileName) + addExtension + subExtension1;
                    newFiles.Add(new DataFilePath(Path.Combine(folder1, fileName1)));
                    break;
                case 2:
                    var folder2 = Path.GetDirectoryName(subtitleFile.FilePath) ?? string.Empty;
                    var subExtension2 = Path.GetExtension(subtitleFile.FileName);
                    var fileName2 = Path.GetFileNameWithoutExtension(mediaFile.FileName) + addExtension + subExtension2;
                    newFiles.Add(new DataFilePath(Path.Combine(folder2, fileName2)));
                    break;
            }
        }

        return newFiles;
    }

    /// <summary>
    /// 文件操作
    /// </summary>
    /// <param name="subtitleFiles">字幕文件路径</param>
    /// <param name="targetPaths">目标路径</param>
    /// <param name="fileOperateMode">文件操作模式 0:转换为SRT 1:复制 2:重命名</param>
    public static async Task RunFileOperates(List<DataFilePath> subtitleFiles, List<DataFilePath> targetPaths, int fileOperateMode)
    {
        var main = App.GetService<MainWindowViewModel>();
        var count = Math.Min(subtitleFiles.Count, targetPaths.Count);
        if (fileOperateMode == 0)
        {
            for (int i = 0; i < count; i++)
            {
                main?.SetGlobalProcess(true, i + 1, count);
                var ext = Path.GetExtension(subtitleFiles[i].FilePath);
                if (ext != ".ass")
                {
                    Logs.LogError($"并非Ass: {subtitleFiles[i].FilePath}");
                    continue;
                }

                await ConvertAssFileToSrt(subtitleFiles[i].FilePath, targetPaths[i].FilePath);
            }

            return;
        }

        await Task.Run(() =>
        {
            for (int i = 0; i < count; i++)
            {
                main?.SetGlobalProcess(true, i + 1, count);

                switch (fileOperateMode)
                {
                    case 1:
                        File.Copy(subtitleFiles[i].FilePath, targetPaths[i].FilePath, true);
                        break;
                    case 2:
                        File.Move(subtitleFiles[i].FilePath, targetPaths[i].FilePath, true);
                        break;
                }
            }
        });
    }

    public static async Task ConvertAssFileToSrt(string assFilePath, string srtFilePath)
    {
        if (!File.Exists(assFilePath))
        {
            Logs.LogError($"字幕文件不存在: {assFilePath}");
            return;
        }

        var tempFilePath = assFilePath + ".temp";
        if (!File.Exists(tempFilePath))
        {
            File.Create(tempFilePath).Close();
        }

        //读取Ass文件,移除特效标签后保存到临时文件
        await using (var tempFile = new FileStream(tempFilePath, FileMode.Truncate, FileAccess.ReadWrite, FileShare.None))
        {
            var lines = await File.ReadAllLinesAsync(assFilePath);
            var writer = new StreamWriter(tempFile);

            await Task.Run(() =>
            {
                foreach (var line in lines)
                {
                    if (AssRemoveLineRegex().IsMatch(line)) continue;

                    var newLine = AssEffectsRegex().Replace(line, string.Empty);
                    if (AssKaraokeRegex().IsMatch(newLine))
                    {
                        newLine = newLine.Replace("Comment: ", "Dialogue: ");
                    }

                    writer.WriteLine(newLine);
                }
            });

            await writer.FlushAsync();
            writer.Close();
            tempFile.Close();
        }

        await FFMpegArguments.FromFileInput(tempFilePath).OutputToFile(srtFilePath).ProcessAsynchronously();

        if (File.Exists(tempFilePath))
        {
            File.Delete(tempFilePath);
        }
    }

    //匹配特效标签
    [GeneratedRegex("{[^}]+}")]
    private static partial Regex AssEffectsRegex();

    //匹配非必要行
    [GeneratedRegex("(Style:)|(,fx,{)")]
    private static partial Regex AssRemoveLineRegex();

    //匹配特效歌词
    [GeneratedRegex("Comment: .*,karaoke,.*")]
    private static partial Regex AssKaraokeRegex();
}