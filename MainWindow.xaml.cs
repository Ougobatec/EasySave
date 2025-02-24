using System.Resources;
using System.Windows;
using EasySave.Views;

namespace EasySave
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static BackupManager BackupManager => BackupManager.GetInstance();          // Backup manager instance
        private static ResourceManager ResourceManager => BackupManager.resourceManager;    // Resource manager instance
        private static MainWindow? MainWindow_Instance;                                     // MainWindow instance

        /// <summary>
        /// MainWindow constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            MainWindow_Instance = this;
            MainFrame.NavigationService.Navigate(new HomePage());
        }

        /// <summary>
        /// Get the MainWindow instance or create it if it doesn't exist
        /// </summary>
        public static MainWindow GetInstance()
        {
            MainWindow_Instance ??= new MainWindow();
            return MainWindow_Instance;
        }

        /// <summary>
        /// Refresh the MainWindow content
        /// </summary>
        public void Refresh()
        {
            Button_Quit.Content = ResourceManager.GetString("Button_Quit");
            Button_Home.Content = ResourceManager.GetString("Button_Home");
            Button_Settings.Content = ResourceManager.GetString("Button_Settings");
            Button_Logs.Content = ResourceManager.GetString("Button_Logs");
        }

        /// <summary>
        /// Button Home click event
        /// </summary>
        private void Button_Home_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.NavigationService.Navigate(new HomePage());
        }

        /// <summary>
        /// Button Settings click event
        /// </summary>
        private void Button_Settings_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.NavigationService.Navigate(new SettingsPage());
        }

        /// <summary>
        /// Button Logs click event
        /// </summary>
        private void Button_Logs_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.NavigationService.Navigate(new LogsPage());
        }

        /// <summary>
        /// Button Quit click event
        /// </summary>
        private void Button_Quit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            Environment.Exit(0);
        }
    }
}
