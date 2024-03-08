namespace BangumiSubReNamer.Models;

public class DataWindowSize
{
    public DataWindowSize(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public int Width { get; set; }
    public int Height { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not DataWindowSize dataWindowSize) return false;
        return Width == dataWindowSize.Width && Height == dataWindowSize.Height;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Height, Width);
    }
}