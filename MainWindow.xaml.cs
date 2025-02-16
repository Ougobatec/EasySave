using System.Windows;

namespace EasySave
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                MainFrame.NavigationService.Navigate(new ManageBackupJobs()); // Charger la première page au démarrage
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement : {ex.Message}");
            }
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
        /// Go to the settings page
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
    }
}
