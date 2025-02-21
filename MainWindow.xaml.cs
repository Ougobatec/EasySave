using System.Configuration;
using System.Resources;
using System.Windows;
using EasySave.Views;
using Microsoft.Windows.Themes;

namespace EasySave
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainWindow? MainWindow_Instance;
        private static ResourceManager ResourceManager => BackupManager.GetInstance().resourceManager;

        public MainWindow()
        {
            BackupManager.GetInstance().Load();

            InitializeComponent();
            MainWindow_Instance = this;

            Refresh();
            MainFrame.NavigationService.Navigate(new HomePage());
        }

        public static MainWindow GetInstance()
        {
            MainWindow_Instance ??= new MainWindow();
            return MainWindow_Instance;
        }

        public void Refresh()
        {
            Button_Quit.Content = ResourceManager.GetString("Button_Quit");
            Button_Home.Content = ResourceManager.GetString("Button_Home");
            Button_Settings.Content = ResourceManager.GetString("Button_Settings");
            Button_Logs.Content = ResourceManager.GetString("Button_Logs");
        }

        private void Button_Home_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.NavigationService.Navigate(new HomePage());
        }

        private void Button_Settings_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.NavigationService.Navigate(new SettingsPage());
        }

        private void Button_Logs_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.NavigationService.Navigate(new LogsPage());
        }

        private void Button_Quit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            Environment.Exit(0);
        }
    }
}