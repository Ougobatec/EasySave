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

        private ServerManager EasySaveServer = new ServerManager();
        private Socket ServerSocket;
        private Socket ClientSocket;
        public ModelConnection ModelConnection { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            MainWindow_Instance = this;
            MainFrame.NavigationService.Navigate(new HomePage());
            DataContext = EasySaveServer.ModelConnection;

            EasySaveServer.ConnectionAccepted += OnConnectionAccepted;
            EasySaveServer.ConnectionStatusChanged += OnConnectionStatusChanged;
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
            EasySaveServer.StopSocketServer(ClientSocket);
            Application.Current.Shutdown();
            Environment.Exit(0);
        }

        private async void ToggleButton_Server_Checked(object sender, RoutedEventArgs e)
        {
            ServerSocket = EasySaveServer.StartSocketServer();
            await EasySaveServer.AcceptConnectionAsync(ServerSocket);
        }

        private void ToggleButton_Server_Unchecked(object sender, RoutedEventArgs e)
        {
            EasySaveServer.StopSocketServer(ServerSocket);
        }

        private void OnConnectionAccepted(object sender, Socket clientSocket)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBoxResult result = MessageBox.Show("Accepter la connexion entrante ?", "Connexion entrante", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    EasySaveServer.ModelConnection.ConnectionStatus = "Connected";
                    Thread listenThread = new Thread(() => EasySaveServer.ListenToClient(clientSocket));
                    listenThread.Start();
                }
                else
                {
                    EasySaveServer.StopSocketServer(clientSocket);
                }
            });
        }

        private void OnConnectionStatusChanged(object sender, string status)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                EasySaveServer.ModelConnection.ConnectionStatus = status;
            });
        }
    }
}
