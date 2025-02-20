using System.Collections.ObjectModel;
using System.IO;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using EasySave.Models;
using Logger;
using Microsoft.Win32;


namespace EasySave.Views
{
    /// <summary>
    /// Logique d'interaction pour EditPage.xaml
    /// </summary>
    public partial class EditPage : Page
    {
        // Used to keep the data on the current job to use it between methods
        private readonly ModelJob? Job = null;
        private int Index;
        private static ResourceManager ResourceManager => BackupManager.GetInstance().resourceManager;
        public ObservableCollection<ModelLog> SavesEntries { get; set; }

        public EditPage(ModelJob? job = null)
        {
            InitializeComponent();
            DataContext = this;
            Job = job;
            Refresh();

            if (job != null)
            {
                LoadJob(job);
            }
        }

        private void Refresh()
        {
            MainWindow.GetInstance().Refresh();
            Title_Edit.Text = ResourceManager.GetString("Title_Edit");
            Text_BackupName.Text = ResourceManager.GetString("Text_BackupName");
            Text_SourceDirectory.Text = ResourceManager.GetString("Text_SourceDirectory");
            Text_TargetDirectory.Text = ResourceManager.GetString("Text_TargetDirectory");
            Text_Type.Text = ResourceManager.GetString("Text_Type");
            Button_Submit.Content = ResourceManager.GetString("Button_Submit");
            Title_SavesList.Text = ResourceManager.GetString("Title_SavesList");
            Header_Date.Header = ResourceManager.GetString("Text_Date");
            Header_BackupName.Header = ResourceManager.GetString("Text_BackupName");
            Header_Type.Header = ResourceManager.GetString("Text_Type");
            Header_Size.Header = ResourceManager.GetString("Text_Size");
        }

        private void SavesDataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                double totalWidth = dataGrid.ActualWidth - SystemParameters.VerticalScrollBarWidth;
                dataGrid.Columns[0].Width = totalWidth * 0.25;  // 25% pour "Horodatage"
                dataGrid.Columns[1].Width = totalWidth * 0.25;  // 25% pour "Nom sauvegarde"
                dataGrid.Columns[2].Width = totalWidth * 0.2;   // 20% pour "Emplacement source"
                dataGrid.Columns[3].Width = totalWidth * 0.3;   // 30% pour "Emplacement cible"
            }
        }
        
        private void LoadJob(ModelJob job)
        {
            Title_Edit.Text = ResourceManager.GetString("Title_Edit") + " - " + job.Name;
            Textbox_BackupName.Text = job.Name;
            Textbox_SourceDirectory.Text = job.SourceDirectory;
            Textbox_TargetDirectory.Text = job.TargetDirectory;
            ComboBox_Type.SelectedIndex = (int)job.Type;
            Title_SavesList.Visibility = Visibility.Visible;
            SavesList.Visibility = Visibility.Visible;
            DisplaySaves();
        }

        private void Button_Submit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = Textbox_BackupName.Text;
                string source = Textbox_SourceDirectory.Text;
                string target = Textbox_TargetDirectory.Text;
                string type = ((ComboBoxItem)ComboBox_Type.SelectedItem)?.Content.ToString();

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target) || string.IsNullOrWhiteSpace(type))
                {
                    MessageBox.Show(ResourceManager.GetString("Message_Fill"), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Warning);
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
                }
                else
                {
                    Index = BackupManager.GetInstance().JsonConfig.BackupJobs.FindIndex(s => s.Name == Job.Name);
                    if (Index != -1)
                    {
                        BackupManager.GetInstance().UpdateBackupJobAsync(job, Index);
                    }
                    else
                    {
                        MessageBox.Show($"Erreur", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                // Réinitialiser les champs après ajout
                Textbox_BackupName.Text = "";
                Textbox_SourceDirectory.Text = "";
                Textbox_TargetDirectory.Text = "";
                ComboBox_Type.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BrowseSourceDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Dossier sélectionné",
                Filter = "Dossiers|*.none",
                Title = "Sélectionner le dossier source"
            };

            if (dialog.ShowDialog() == true)
            {
                Textbox_SourceDirectory.Text = System.IO.Path.GetDirectoryName(dialog.FileName);
            }
        }

        private void BrowseTargetDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Dossier sélectionné",
                Filter = "Dossiers|*.none",
                Title = "Sélectionner le dossier de destination"
            };

            if (dialog.ShowDialog() == true)
            {
                Textbox_TargetDirectory.Text = System.IO.Path.GetDirectoryName(dialog.FileName);
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
