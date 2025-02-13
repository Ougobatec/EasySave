using EasySave.Models;
using System.Collections.ObjectModel;
using System.Resources;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasySave
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        

        public MainWindow()
        {
            InitializeComponent();
            
            
            MainFrame.NavigationService.Navigate(new ManageBackupJobs()); // Charger la première page au démarrage
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
            MainFrame.NavigationService.Navigate(new Settings());
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
        }
    }
}