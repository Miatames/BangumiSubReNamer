using System.IO;

namespace BangumiMediaTool.Models;

public class DataFilePath
{
    public DataFilePath(string filePath)
    {
        FilePath = filePath;
        FileName = Path.GetFileName(filePath);
    }

    public string FilePath { get; }
    public string FileName { get; }


    public override bool Equals(object? obj)
    {
        if (obj is not DataFilePath dataFilePath) return false;
        return FilePath == dataFilePath.FilePath && FileName == dataFilePath.FileName;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FilePath, FileName);
    }

    public override string ToString()
    {
        return $"{FilePath} {FileName}";
    }
}