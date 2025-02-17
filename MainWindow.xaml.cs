using System.Globalization;
using System.Resources;
using System.Windows;

namespace EasySave
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainWindow MainWindow_Instance = null;
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                MainWindow_Instance = this; // Stocke l'instance active de MainWindow
                MainFrame.NavigationService.Navigate(new ManageBackupJobs()); // Charger la première page au démarrage
                QuitButton.Content = BackupManager.GetInstance().resourceManager.GetString("Menu_Quit");
                HomeButton.Content = BackupManager.GetInstance().resourceManager.GetString("Menu_Home");
                Settings.Content = BackupManager.GetInstance().resourceManager.GetString("Menu_ChangeSettings");
                Logs.Content = BackupManager.GetInstance().resourceManager.GetString("Menu_Logs");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement : {ex.Message}");
            }
        }
        public static MainWindow GetInstance()
        {
            MainWindow_Instance ??= new MainWindow();
            return MainWindow_Instance;
        }
        /// <summary>
        /// Go back to the main page
        /// </summary>
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.NavigationService.Navigate(new ManageBackupJobs());
        }
        /// <summary>
        /// Go to the settings page
        /// </summary>
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            //MainFrame.NavigationService.Navigate(new Settings());

            var settingsPage = new Settings();
            MainFrame.NavigationService.Navigate(settingsPage);

            // remplissage après que la navigation soit terminée
            settingsPage.Settings_Loaded();
        }
        /// <summary>
        /// Go to the logs page
        /// </summary>
        private void Logs_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.NavigationService.Navigate(new Logs());
        }
        /// <summary>
        /// Close the software
        /// </summary>
        private void CloseApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            Environment.Exit(0); // Force la fermeture du processus

        }
        /// <summary>
        /// Refresh the UI
        /// </summary>
        public void RefreshUI()
        {
            QuitButton.Content = BackupManager.GetInstance().resourceManager.GetString("Menu_Quit");
            HomeButton.Content = BackupManager.GetInstance().resourceManager.GetString("Menu_Home");
            Settings.Content = BackupManager.GetInstance().resourceManager.GetString("Menu_ChangeSettings");
            Logs.Content = BackupManager.GetInstance().resourceManager.GetString("Menu_Logs");
        }
    }
}
