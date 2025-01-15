using System.Diagnostics;
using BangumiMediaTool.Models;
using CommunityToolkit.Mvvm.Messaging;
using NLog;
using Wpf.Ui.Controls;

namespace BangumiMediaTool.Services.Program;

public static class Logs
{
    // private static readonly Logger log = LogManager.GetCurrentClassLogger();

    public static void LogInfo(string? message)
    {
        Console.WriteLine(message);
        // log.Info(message);
    }

    public static void LogError(string? message)
    {
        Console.WriteLine(message);
        // log.Error(message);
        WeakReferenceMessenger.Default.Send(new DataSnackbarMessage("错误", message ?? string.Empty, ControlAppearance.Caution,60));
    }
}
