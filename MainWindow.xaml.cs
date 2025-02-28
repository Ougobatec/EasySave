using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Sockets;
using System.Resources;
using System.Windows;
using EasySave.Models;
using EasySave.Views;

namespace EasySave
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static BackupManager BackupManager => BackupManager.GetInstance();
        private static ResourceManager ResourceManager => BackupManager.resourceManager;
        public static ServerManager ServerManager => ServerManager.GetInstance();
        private static MainWindow? MainWindow_Instance;

        public MainWindow()
        {
            InitializeComponent();
            MainWindow_Instance = this;
            MainFrame.NavigationService.Navigate(new HomePage());
            DataContext = ServerManager.Connection;
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
            if (ServerManager.Connection.ServerStatus)
            {
                ToggleButton_Server.Content = ResourceManager.GetString("Button_Stop");
            }
            else
            {
                ToggleButton_Server.Content = ResourceManager.GetString("Button_Start");
            }
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
            ServerManager.StopServer();
            Application.Current.Shutdown();
            Environment.Exit(0);
        }

        private void ToggleButton_Server_Checked(object sender, RoutedEventArgs e)
        {
            ServerManager.StartServer();
            ToggleButton_Server.Content = ResourceManager.GetString("Button_Stop");
        }

        private void ToggleButton_Server_Unchecked(object sender, RoutedEventArgs e)
        {
            ServerManager.StopServer();
            ToggleButton_Server.Content = ResourceManager.GetString("Button_Start");
        }
    }
}
