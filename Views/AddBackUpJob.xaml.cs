using EasySave.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace EasySave
{
    /// <summary>
    /// Logique d'interaction pour Page1.xaml
    /// </summary>
    public partial class AddBackUpJob : Page
    {
        private readonly ModelJob? Job = null;
        private int Index;
        public ModelConfig Config { get; private set; }
        public AddBackUpJob(ModelJob? job = null)
        {
            InitializeComponent();
            if (job != null)
            {
                Job = job;
                // Remplir les champs avec les valeurs actuelles
                BackupNameTextBox.Text = job.Name;
                SourceDirectoryTextBox.Text = job.SourceDirectory;
                TargetDirectoryTextBox.Text = job.TargetDirectory;
                TypeComboBox.Text = job.Type.ToString();
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

                Index = BackupManager.GetInstance().Config.BackupJobs.FindIndex(s => s.Name == Job.Name);

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
