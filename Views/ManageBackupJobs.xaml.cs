using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Logique d'interaction pour Page1.xaml
    /// </summary>
    public partial class ManageBackupJobs : Page
    {
        public ManageBackupJobs()
        {
            InitializeComponent();
        }

        private void BackupJobsListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (BackupJobsListView.View is GridView gridView)
            {
                double totalWidth = BackupJobsListView.ActualWidth - SystemParameters.VerticalScrollBarWidth; // Largeur disponible
                double proportion1 = 0.2;  // 20% pour "Nom de sauvegarde"
                double proportion2 = 0.3;  // 30% pour "Emplacement source"
                double proportion3 = 0.3;  // 30% pour "Emplacement cible"
                double proportion4 = 0.1;  // 10% pour "Type"
                double fixedWidth = 100;   // Largeur fixe pour "Modifier"

                gridView.Columns[0].Width = totalWidth * proportion1;
                gridView.Columns[1].Width = totalWidth * proportion2;
                gridView.Columns[2].Width = totalWidth * proportion3;
                gridView.Columns[3].Width = totalWidth * proportion4;
                gridView.Columns[4].Width = fixedWidth;
            }
        }

        /// <summary>
        /// Delete all selected backUps
        /// </summary>
        private void DeleteBackUps_Click(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// Execute all selected backUps
        /// </summary>
        private void ExecuteBackUps_Click(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// Display the page to create a backUpJob
        /// </summary>
        private void CreateBackupJob_Click(object sender, RoutedEventArgs e)
        {
            AddBackUpJob addBackUpJob = new AddBackUpJob();
            this.NavigationService.Navigate(addBackUpJob);
        }
    }
}
