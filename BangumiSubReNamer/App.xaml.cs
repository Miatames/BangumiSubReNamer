using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using BangumiSubReNamer.Models;
using BangumiSubReNamer.Services;
using BangumiSubReNamer.ViewModels.Pages;
using BangumiSubReNamer.ViewModels.Windows;
using BangumiSubReNamer.Views.Pages;
using BangumiSubReNamer.Views.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace BangumiSubReNamer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)); })
            .ConfigureServices((context, services) =>
            {
                services.AddHostedService<ApplicationHostService>();

                services.AddSingleton<IPageService, PageService>();

                services.AddSingleton<IThemeService, ThemeService>();

                services.AddSingleton<ITaskBarService, TaskBarService>();

                services.AddSingleton<INavigationService, NavigationService>();

                services.AddSingleton<INavigationWindow, MainWindow>();
                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton<FilePreviewWindow>();

                services.AddSingleton<MediaRenamerPage>();
                services.AddSingleton<MediaRenamerViewModel>();
                services.AddSingleton<MediaDataPage>();
                services.AddSingleton<MediaDataViewModel>();
                services.AddSingleton<SubRenamerPage>();
                services.AddSingleton<SubRenamerViewModel>();
                services.AddSingleton<QbtRssPage>();
                services.AddSingleton<QbtRssViewModel>();
                services.AddSingleton<SettingsPage>();
                services.AddSingleton<SettingsViewModel>();
            }).Build();

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetService<T>()
            where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private void OnStartup(object sender, StartupEventArgs e)
        {
            _host.Start();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();

            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            WeakReferenceMessenger.Default.Send<DataSnackbarMessage>(
                new DataSnackbarMessage("错误", e.Exception.Message, ControlAppearance.Caution));
            e.Handled = true;
        }
    }
}