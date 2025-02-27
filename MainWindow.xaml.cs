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
        private static MainWindow? MainWindow_Instance;

        private ServerManager ServerManager =>  ServerManager.GetInstance();
        //private ServerManager EasySaveServer => ServerManager.GetInstance();
        private Socket ServerSocket;
        private Socket ClientSocket;
        

        public MainWindow()
        {
            InitializeComponent();
            MainWindow_Instance = this;
            MainFrame.NavigationService.Navigate(new HomePage());
            DataContext = ServerManager.ModelConnection;

            ServerManager.ConnectionAccepted += OnConnectionAccepted;
            ServerManager.ConnectionStatusChanged += OnConnectionStatusChanged;
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
            ServerManager.StopSocketServer(ClientSocket);
            Application.Current.Shutdown();
            Environment.Exit(0);
        }

        private async void ToggleButton_Server_Checked(object sender, RoutedEventArgs e)
        {
            ServerSocket = ServerManager.StartSocketServer();
            await ServerManager.AcceptConnectionAsync(ServerSocket);
        }

        private void ToggleButton_Server_Unchecked(object sender, RoutedEventArgs e)
        {
            ServerManager.StopSocketServer(ServerSocket);
        }

        private void OnConnectionAccepted(object sender, Socket clientSocket)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ServerManager.GetInstance().ModelConnection.Client = clientSocket; // Use the type name instead of the instance
                MessageBoxResult result = MessageBox.Show("Accepter la connexion entrante ?", "Connexion entrante", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    ServerManager.ModelConnection.ConnectionStatus = "Connected";

                    Thread listenThread = new Thread(() => ServerManager.ListenToClient());
                    listenThread.Start();
                }
                else
                {
                    ServerManager.StopSocketServer(ServerManager.GetInstance().ModelConnection.Client);
                }
            });
        }

        private void OnConnectionStatusChanged(object sender, string status)
         {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ServerManager.ModelConnection.ConnectionStatus = status;
            });
        }
    }
}
