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
        private readonly BackupManager backupManager = new();
        private ResourceManager ResourceManager => backupManager.resourceManager;
        private bool exit = false;
        public ObservableCollection<ModelJob> BackupJobs { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            DisplayBackupJobs();
            MainFrame.NavigationService.Navigate(new ManageBackupJobs()); // Charger la première page au démarrage
        }

        //afficher les jobs de backup dans le datagrid
        private void DisplayBackupJobs()
        {
            Console.WriteLine();
            BackupJobs = new ObservableCollection<ModelJob>();

            for (int i = 0; i < backupManager.Config.BackupJobs.Count; i++)
            {
                var job = backupManager.Config.BackupJobs[i];
                Console.WriteLine(string.Format(ResourceManager.GetString("Message_BackupJobDetails"), i, job.Name, job.SourceDirectory, job.TargetDirectory, job.Type));
                BackupJobs.Add(new ModelJob
                {
                    Name = job.Name,
                    SourceDirectory = job.SourceDirectory,
                    TargetDirectory = job.TargetDirectory,
                    Type = job.Type
                });
            }

            // Si vous avez une liaison de données dans XAML, vous devrez peut-être rafraîchir l'interface utilisateur
            // Exemple : dataGrid.ItemsSource = BackupJobs;
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