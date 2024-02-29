using Wpf.Ui.Controls;

namespace BangumiSubReNamer.Models;

public class DataSnackbarMessage
{
    public DataSnackbarMessage(string title, string message, ControlAppearance controlAppearance)
    {
        Title = title;
        Message = message;
        ControlAppearance = controlAppearance;
    }

    public string Title { get; set; }

    public string Message { get; set; }
    
    public ControlAppearance ControlAppearance { get; set; }
}