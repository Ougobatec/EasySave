using EasySave.Models;
using System.Collections.ObjectModel;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;


namespace EasySave
{
    /// <summary>
    /// Logique d'interaction pour Page1.xaml
    /// </summary>
    public partial class ManageBackupJobs : Page
    {
        private bool exit = false;
        public ObservableCollection<ModelJob> BackupJobs { get; set; }
        public ObservableCollection<ModelState> BackupStates { get; set; }

        public ManageBackupJobs()
        {
            InitializeComponent();
            DataContext = this;
            DisplayBackupJobs();
        }

        private void BackupJobsListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (BackupJobsListView.View is GridView gridView)
            {
                double totalWidth = BackupJobsListView.ActualWidth - SystemParameters.VerticalScrollBarWidth; // Largeur disponible
                double proportion1 = 0.2;  // 20% pour "Nom de sauvegarde"
                double proportion2 = 0.3;  // 30% pour "Emplacement source"
                double proportion3 = 0.2;  // 20% pour "Emplacement cible"
                double proportion4 = 0.1;  // 10% pour "Type"

                double proportion5 = 0.1;   // 10% pour "Modifier"
                double proportion6 = 0.1;   // 10% pour "Etat"


                gridView.Columns[0].Width = totalWidth * proportion1;
                gridView.Columns[1].Width = totalWidth * proportion2;
                gridView.Columns[2].Width = totalWidth * proportion3;
                gridView.Columns[3].Width = totalWidth * proportion4;
                gridView.Columns[4].Width = totalWidth * proportion5;
                gridView.Columns[5].Width = totalWidth * proportion6;
            }
        }

        /// <summary>
        /// Delete all selected backUps
        /// </summary>
        private void DeleteBackUps_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedItems = BackupJobsListView.SelectedItems.Cast<ModelJob>().ToList();

                if (selectedItems.Count == 0)
                {
                    MessageBox.Show("Veuillez sélectionner au moins une sauvegarde à supprimer.", "Aucune sélection", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var jobsToDelete = new List<ModelJob>(selectedItems);

                // Demander confirmation avant de supprimer
                var result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer les sauvegardes sélectionnées ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Supprime les éléments sélectionnés de la liste
                    foreach (var job in jobsToDelete)
                    {
                        int index = BackupJobs.IndexOf(job);
                        if (index >= 0)
                        {
                            BackupJobs.RemoveAt(index);
                            BackupManager.GetInstance().DeleteBackupJobAsync(index);
                        }
                    }

                    // Rafraîchir la ListView
                    BackupJobsListView.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Une erreur est survenue lors de la suppression des sauvegardes.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Execute all selected backUps
        /// </summary>
        private void ExecuteBackUps_Click(object sender, RoutedEventArgs e)
        {
            // Logique pour exécuter les sauvegardes sélectionnées
            try
            {
                // Test si des logiciels métiers sont ouverts
                if (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
                {
                    MessageBox.Show("Un logiciel métier est en cours d'exécution. Veuillez le fermer avant de lancer une sauvegarde.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    var selectedItems = BackupJobsListView.SelectedItems.Cast<ModelJob>().ToList();

                    if (selectedItems.Count == 0)
                    {
                        MessageBox.Show("Veuillez sélectionner au moins une sauvegarde à exécuter.", "Aucune sélection", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var jobsToExecute = new List<ModelJob>(selectedItems);

                    // Demander confirmation avant de lancer l'éxecution
                    var result = MessageBox.Show($"Veuillez confirmer l'exécution des sauvegardes.", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        // Execute² les éléments sélectionnés de la liste
                        foreach (var job in jobsToExecute)
                        {
                            int index = BackupJobs.IndexOf(job);
                            if (index >= 0)
                            {
                                BackupManager.GetInstance().ExecuteBackupJobAsync(index);
                            }
                        }

                        // Rafraîchir la ListView
                        BackupJobsListView.Items.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Une erreur est survenue lors de la suppression des sauvegardes.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Display the page to create or edit a backUpJob
        /// </summary>
        private void CreateBackupJob_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                // Sert à naviguer vers la page AddBackUpJob
                AddEditBackUpJob addBackUpJob = new AddEditBackUpJob();
                this.NavigationService.Navigate(addBackUpJob);
                Button button = sender as Button;
                if (button != null && button.DataContext is ModelJob selectedJob)
                {
                    // Navigation vers la page de modification avec les données
                    NavigationService.Navigate(new AddEditBackUpJob(selectedJob));
                }
            }
        }

        // Afficher les jobs de backup dans le ListView
        private void DisplayBackupJobs()
        {
            BackupJobs = new ObservableCollection<ModelJob>(BackupManager.GetInstance().Config.BackupJobs);

        }
        
       
    }
}
