using EasySave.Models;
using Logger;
using System.Collections.ObjectModel;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System;
using System.IO;
using System.Linq.Expressions;

namespace EasySave
{
    /// <summary>
    /// Logique d'interaction pour Page1.xaml
    /// </summary>
    public partial class AddEditBackUpJob : Page
    {
        // Used to keep the data on the current job to use it between methods
        private readonly ModelJob? Job = null;
        private int Index;
        public ModelConfig Config { get; private set; }
        private static ResourceManager ResourceManager => BackupManager.GetInstance().resourceManager;
        public ObservableCollection<ModelLog> SavesEntries { get; set; }

        public AddEditBackUpJob(ModelJob? job = null)
        {
            InitializeComponent();
            DataContext = this;
            // Change languages
            BackupNameTextBlock.Text = ResourceManager.GetString("Prompt_JobName");
            SourceDirectoryTextBlock.Text = ResourceManager.GetString("Prompt_SourceDirectory");
            TargetDirectoryTextBlock.Text = ResourceManager.GetString("Prompt_TargetDirectory");
            TypeTextBlock.Text = ResourceManager.GetString("Prompt_BackupType");
            TitleAddEditBackupJob.Text = ResourceManager.GetString("Title_Add_Backup");
            TitleSavesList.Text = ResourceManager.GetString("TitleSavesList");
            Button_Submit.Content = ResourceManager.GetString("Button_Submit");
            Header_Saves_Name.Header = ResourceManager.GetString("Header_Saves_Name");
            Header_Saves_Type.Header = ResourceManager.GetString("Header_Saves_Type");
            Header_Saves_Size.Header = ResourceManager.GetString("Header_Saves_Size");
            Header_Saves_Date.Header = ResourceManager.GetString("Header_Saves_Date");

            SavesList.Visibility = Visibility.Hidden;
            if (job != null)
            {
                Job = job;
                DisplaySaves();
                // Remplir les champs avec les valeurs actuelles
                BackupNameTextBox.Text = job.Name;
                SourceDirectoryTextBox.Text = job.SourceDirectory;
                TargetDirectoryTextBox.Text = job.TargetDirectory;
                TypeComboBox.Text = job.Type.ToString();
                TitleAddEditBackupJob.Text = ResourceManager.GetString("Title_Edit_Backup")+ " " + job.Name;
                SavesList.Visibility = Visibility.Visible;
            }
        }
        private void SavesDataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                double totalWidth = dataGrid.ActualWidth - SystemParameters.VerticalScrollBarWidth; // Largeur disponible
                double proportion1 = 0.25;  // 25% pour "Horodatage"
                double proportion2 = 0.25;  // 25% pour "Nom sauvegarde"
                double proportion3 = 0.20;  // 20% pour "Emplacement source"
                double proportion4 = 0.3;  // 30% pour "Emplacement cible"

                dataGrid.Columns[0].Width = totalWidth * proportion1;
                dataGrid.Columns[1].Width = totalWidth * proportion2;
                dataGrid.Columns[2].Width = totalWidth * proportion3;
                dataGrid.Columns[3].Width = totalWidth * proportion4;
            }
        }
        private void ConfirmationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = BackupNameTextBox.Text;
                string source = SourceDirectoryTextBox.Text;
                string target = TargetDirectoryTextBox.Text;
                string type = ((ComboBoxItem)TypeComboBox.SelectedItem)?.Content.ToString();

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target) || string.IsNullOrWhiteSpace(type))
                {
                    MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var job = new ModelJob
                {
                    Name = name,
                    SourceDirectory = source,
                    TargetDirectory = target,
                    Type = Enum.Parse<EasySave.Enumerations.BackupTypes>(type),
                };
                if (Job == null)
                {
                    BackupManager.GetInstance().AddBackupJobAsync(job);
                    // Confirmer à l'utilisateur
                    MessageBox.Show("Sauvegarde ajoutée avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Index = BackupManager.GetInstance().Config.BackupJobs.FindIndex(s => s.Name == Job.Name);
                    if (Index != -1)
                    {
                        BackupManager.GetInstance().UpdateBackupJobAsync(job, Index);
                        // Confirmer à l'utilisateur
                        MessageBox.Show("Les modifications ont été enregistrées !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Erreur", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                // Réinitialiser les champs après ajout
                BackupNameTextBox.Text = "";
                SourceDirectoryTextBox.Text = "";
                TargetDirectoryTextBox.Text = "";
                TypeComboBox.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void DisplaySaves()
        {
            SavesEntries = new ObservableCollection<ModelLog>(await Logger<ModelLog>.GetInstance().GetLogs());

            foreach (ModelLog el in SavesEntries.ToList()) // Convertir en liste temporaire pour éviter la modification pendant l'itération
            {
                // we check for saves that are not associated with the backUpJob to remove them
                // !Directory.Exists(el.Destination) : we go check if the path exist or not (save could have been deleted)
                // Job.TargetDirectory != Path.GetDirectoryName(el.Destination.TrimEnd('\\')) || Job.Name != el.BackupName : we check if the folder above is different than backUpJob directory or if the name of the backupJob is different
                if (Job != null && (!Directory.Exists(el.Destination) || Job.TargetDirectory != Path.GetDirectoryName(el.Destination.TrimEnd('\\')) || Job.Name != el.BackupName))
                {
                    SavesEntries.Remove(el);
                }
                else
                {
                    // we check the type of the saves with the folder name
                    string type = Path.GetFileName(el.Destination.TrimEnd(Path.DirectorySeparatorChar));
                    el.BackupName = type;
                    if (type.Contains("full"))
                    {
                        el.Source = "Full";
                    }
                    else if (type.Contains("diff"))
                    {
                        el.Source = "Differential";
                    }
                    else
                    {
                        el.Source = "";
                    }
                }
            }
        }
    }
}
