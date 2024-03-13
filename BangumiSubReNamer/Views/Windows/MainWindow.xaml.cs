using System.IO;
using BangumiSubReNamer.Models;
using BangumiSubReNamer.Services;
using BangumiSubReNamer.ViewModels.Pages;
using BangumiSubReNamer.ViewModels.Windows;
using BangumiSubReNamer.Views.Pages;
using CommunityToolkit.Mvvm.Messaging;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace BangumiSubReNamer.Views.Windows
{
    public partial class MainWindow : INavigationWindow, IRecipient<DataSnackbarMessage>,IRecipient<DataFilePathPreview>
    {
        public MainWindowViewModel ViewModel { get; }
        public static MainWindow Instance; 

        private GlobalConfig globalConfig;
        private BangumiApiConfig bangumiApiConfig;
        private readonly ISnackbarService snackbarService;
        private StreamWriter consoleStreamWriter;

        private MediaRenamerPage mediaRenamerPage;
        private MediaDataPage mediaDataPage;
        private SubRenamerPage subRenamerPage;
        private SettingsPage settingsPage;

        public MainWindow(
            MainWindowViewModel viewModel,
            IPageService pageService,
            INavigationService navigationService
        )
        {
            Instance = this;
            ViewModel = viewModel;
            DataContext = this;

            SystemThemeWatcher.Watch(this);

            InitializeComponent();
            SetPageService(pageService);
            navigationService.SetNavigationControl(RootNavigation);

            Console.WriteLine("start");

            bangumiApiConfig = new BangumiApiConfig();
            globalConfig = new GlobalConfig();
            snackbarService = new SnackbarService();
            snackbarService.SetSnackbarPresenter(UI_SnackbarPresenter);
            // CreateLogFile();

            WeakReferenceMessenger.Default.Register<DataSnackbarMessage>(this);
            WeakReferenceMessenger.Default.Register<DataFilePathPreview>(this);
        }

        #region INavigationWindow methods

        public INavigationView GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(IPageService pageService) => RootNavigation.SetPageService(pageService);

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion INavigationWindow methods

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // consoleStreamWriter.Flush();
            // consoleStreamWriter.Close();

            Application.Current.Shutdown();
        }

        INavigationView INavigationWindow.GetNavigation()
        {
            return RootNavigation;
        }

        public void SetServiceProvider(IServiceProvider serviceProvider) { }

        public void Receive(DataSnackbarMessage message)
        {
            snackbarService.Show(message.Title, message.Message, message.ControlAppearance, null,
                TimeSpan.FromSeconds(2));
        }

        private void CreateLogFile()
        {
            var logFilePath = $"log\\log_{DateTime.Now.Date:yy-MM-dd}.txt";
            Console.WriteLine(logFilePath);

            if (!Directory.Exists("log")) Directory.CreateDirectory("log");
            if (!File.Exists(logFilePath))
            {
                FileStream stream = new FileStream(logFilePath, FileMode.Create);
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(logFilePath);
                writer.Close();
                stream.Close();
            }

            consoleStreamWriter = new StreamWriter(logFilePath);
            Console.SetOut(consoleStreamWriter);
        }

        public void Receive(DataFilePathPreview message)
        {
            var window = new FilePreviewWindow(new FilePreviewWindowViewModel(message))
            {
                Owner = this,
                ShowInTaskbar = false
            };
            window.ShowDialog();
        }
    }
}