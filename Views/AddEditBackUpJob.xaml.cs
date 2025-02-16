using EasySave.Models;
using System.Windows;
using System.Windows.Controls;

namespace EasySave
{
    /// <summary>
    /// Logique d'interaction pour Page1.xaml
    /// </summary>
    public partial class AddEditBackUpJob : Page
    {
        private readonly ModelJob? Job = null;
        private int Index;
        public ModelConfig Config { get; private set; }
        public AddEditBackUpJob(ModelJob? job = null)
        {
            InitializeComponent();
            TitleAddEditBackupJob.Text = "Ajouter un travail de sauvegarde";
            SavesList.Visibility = Visibility.Hidden;
            Job = job;
            if (job != null)
            {
                // Remplir les champs avec les valeurs actuelles
                BackupNameTextBox.Text = job.Name;
                SourceDirectoryTextBox.Text = job.SourceDirectory;
                TargetDirectoryTextBox.Text = job.TargetDirectory;
                TypeComboBox.Text = job.Type.ToString();
                TitleAddEditBackupJob.Text = "Modifier " + job.Name;
                SavesList.Visibility = Visibility.Visible;
            }
        }
        private void SavesListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (SavesListView.View is GridView gridView)
            {
                double totalWidth = SavesListView.ActualWidth - SystemParameters.VerticalScrollBarWidth; // Largeur disponible
                double proportion1 = 0.40;  // 25% pour "Nom sauvegarde"
                double proportion2 = 0.20;  // 20% pour "Type"
                double proportion3 = 0.20;  // 20% pour "Taille"
                double proportion4 = 0.20;  // 20% pour "Date"

                //double proportion5 = 0.1;   // 10% pour "Modifier"
                //double proportion6 = 0.1;   // 10% pour "Etat"


                gridView.Columns[0].Width = totalWidth * proportion1;
                gridView.Columns[1].Width = totalWidth * proportion2;
                gridView.Columns[2].Width = totalWidth * proportion3;
                gridView.Columns[3].Width = totalWidth * proportion4;
                //gridView.Columns[4].Width = totalWidth * proportion5;
                //gridView.Columns[5].Width = totalWidth * proportion6;
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
    }
}
