using System.Diagnostics.CodeAnalysis;

namespace BangumiSubReNamer.Models;

public class DataFilePath
{
    public DataFilePath(string filePath, string fileName)
    {
        FilePath = filePath;
        FileName = fileName;
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