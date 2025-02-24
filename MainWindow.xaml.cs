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
        private static BackupManager BackupManager => BackupManager.GetInstance();          // Backup manager instance
        private static ResourceManager ResourceManager => BackupManager.resourceManager;    // Resource manager instance
        private static MainWindow? MainWindow_Instance;                                     // MainWindow instance
        
        
        
        private EasySaveServer EasySaveServer = new EasySaveServer();
        private Socket ServerSocket;
        private Socket ClientSocket;
        public ModelConnection Connection { get; set; }





        /// <summary>
        /// MainWindow constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            
            MainWindow_Instance = this;
            MainFrame.NavigationService.Navigate(new HomePage());

            //Connection = new ModelConnection();
            DataContext = EasySaveServer.ModelConnection;
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
            EasySaveServer.StopSocketServer(ClientSocket);
            Application.Current.Shutdown();
            Environment.Exit(0);
        }

















        private async void Button_Start_Server_Click(object sender, RoutedEventArgs e)
        {
            ServerSocket = EasySaveServer.StartSocketServer(12345);

            // Attendre une connexion de manière asynchrone
            ClientSocket = await EasySaveServer.AcceptConnectionAsync(ServerSocket);

            // Afficher une MessageBox pour demander à l'utilisateur s'il souhaite accepter la connexion
            MessageBoxResult result = MessageBox.Show("Accepter la connexion entrante ?", "Connexion entrante", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                EasySaveServer.ModelConnection.ConnectionStatus = "Connected";
                Thread Listen = new Thread(()=>EasySaveServer.ListenToClient(ClientSocket)); // Commencer à écouter le client
            }
            else
            {
                EasySaveServer.ModelConnection.ConnectionStatus = "Not Connected";
                EasySaveServer.StopSocketServer(ClientSocket); // Fermer la connexion si refusée
            }
        }

        //private void DisplayConnectionStatus()
        //{
        //    Connections = new ObservableCollection<ModelConnection> { EasySaveServer.ModelConnection };
        //}





    }
}
