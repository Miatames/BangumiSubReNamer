using Wpf.Ui.Controls;

namespace BangumiMediaTool.Models;

public class DataSnackbarMessage
{
    public DataSnackbarMessage(string title, string message, ControlAppearance controlAppearance)
    {
        Title = title;
        Message = message;
        ControlAppearance = controlAppearance;
    }

    public DataSnackbarMessage(string title, string message, ControlAppearance controlAppearance, long seconds)
    {
        Title = title;
        Message = message;
        ControlAppearance = controlAppearance;
        Seconds = seconds;
    }

    public string Title { get; set; }

    public string Message { get; set; }
    
    public ControlAppearance ControlAppearance { get; set; }

    public long Seconds { get; set; } = 2;
}