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
        private static readonly string ConfigFilePath = Path.Join(Path.GetTempPath(), "easysave\\config.json");
        public ModelConfig Config { get; private set; }
        public AddBackUpJob(ModelJob? job = null)
        {
            InitializeComponent();
            if (job != null)
            {
                // Remplir les champs avec les valeurs actuelles
                BackupNameTextBox.Text = job.Name;
                SourceDirectoryTextBox.Text = job.SourceDirectory;
                TargetDirectoryTextBox.Text = job.TargetDirectory;
                TypeComboBox.SelectedItem = job.Type;
            }
            else
            {

            }
        }
        private void ConfirmationButton_Click(object sender, RoutedEventArgs e)
        {
            //ModelJob job = new ModelJob();
            //job.Name = BackupNameTextBox.Text;
            //job.SourceDirectory = SourceDirectoryTextBox.Text;
            //job.TargetDirectory = TargetDirectoryTextBox.Text;
            //job.Type = (BackupTypes)Enum.Parse(typeof(BackupTypes), ((ComboBoxItem)TypeComboBox.SelectedItem).Content.ToString());

            //var json = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true });
            //await File.WriteAllTextAsync(ConfigFilePath, json);

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

                BackupManager.GetInstance().AddBackupJobAsync(job);

                //var json = File.ReadAllText(ConfigFilePath);
                //Config = JsonSerializer.Deserialize<ModelConfig>(json, new JsonSerializerOptions
                //{
                //    PropertyNameCaseInsensitive = true,
                //    ReadCommentHandling = JsonCommentHandling.Skip,
                //    AllowTrailingCommas = true
                //}) ?? new ModelConfig { BackupJobs = [] };

                //// ✅ 3. Charger les travaux existants depuis le fichier JSON
                //List<ModelJob> jobs = new List<ModelJob>();
                //if (File.Exists(ConfigFilePath))
                //{
                //    string json = File.ReadAllText(ConfigFilePath);
                //    jobs = JsonSerializer.Deserialize<List<ModelJob>>(json) ?? new List<ModelJob>();
                //}

                //// ✅ 4. Ajouter le nouveau travail
                //jobs.Add(job);

                //// ✅ 5. Sauvegarder la liste mise à jour dans le fichier JSON
                //string updatedJson = JsonSerializer.Serialize(jobs, new JsonSerializerOptions { WriteIndented = true });
                //File.WriteAllText(ConfigFilePath, updatedJson);

                // ✅ 6. Confirmer à l'utilisateur
                MessageBox.Show("Sauvegarde ajoutée avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                // Optionnel : Réinitialiser les champs après ajout
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
