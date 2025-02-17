using EasySave.Models;
using System.Resources;
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
        private ResourceManager ResourceManager => BackupManager.GetInstance().resourceManager;
        public AddEditBackUpJob(ModelJob? job = null)
        {
            InitializeComponent();
            // Change languages
            BackupNameTextBlock.Text = ResourceManager.GetString("Prompt_JobName");
            SourceDirectoryTextBlock.Text = ResourceManager.GetString("Prompt_SourceDirectory");
            TargetDirectoryTextBlock.Text = ResourceManager.GetString("Prompt_TargetDirectory");
            TypeTextBlock.Text = ResourceManager.GetString("Prompt_BackupType");
            TitleAddEditBackupJob.Text = ResourceManager.GetString("Title_Add_Backup");
            Button_Submit.Content = ResourceManager.GetString("Button_Submit");
            Header_Saves_Name.Header = ResourceManager.GetString("Header_Saves_Name");
            Header_Saves_Type.Header = ResourceManager.GetString("Header_Saves_Type");
            Header_Saves_Size.Header = ResourceManager.GetString("Header_Saves_Size");
            Header_Saves_Date.Header = ResourceManager.GetString("Header_Saves_Date");

            SavesList.Visibility = Visibility.Hidden;
            Job = job;
            if (job != null)
            {
                // Remplir les champs avec les valeurs actuelles
                BackupNameTextBox.Text = job.Name;
                SourceDirectoryTextBox.Text = job.SourceDirectory;
                TargetDirectoryTextBox.Text = job.TargetDirectory;
                TypeComboBox.Text = job.Type.ToString();
                TitleAddEditBackupJob.Text = ResourceManager.GetString("Title_Edit_Backup") + job.Name;
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

                gridView.Columns[0].Width = totalWidth * proportion1;
                gridView.Columns[1].Width = totalWidth * proportion2;
                gridView.Columns[2].Width = totalWidth * proportion3;
                gridView.Columns[3].Width = totalWidth * proportion4;
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
