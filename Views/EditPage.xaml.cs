using System.Collections.ObjectModel;
using System.IO;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using EasySave.Enumerations;
using EasySave.Models;
using Microsoft.Win32;

namespace EasySave.Views
{
    /// <summary>
    /// Interaction logic for EditPage.xaml
    /// </summary>
    public partial class EditPage : Page
    {
        private static BackupManager BackupManager => BackupManager.GetInstance();          // Backup manager instance
        private static ResourceManager ResourceManager => BackupManager.resourceManager;    // Resource manager instance
        public ObservableCollection<ModelSave> SavesEntries { get; set; } = [];             // List to get all saves
        private readonly ModelJob? Job = null;                                              // Job to edit

        /// <summary>
        /// EditPage constructor to initialize the page and display the job if there's one
        /// <param name="job">The job to edit</param>
        /// </summary>
        public EditPage(ModelJob? job = null)
        {
            InitializeComponent();
            DataContext = this;
            Job = job;
            Refresh();
        }

        /// <summary>
        /// Refresh the EditPage content
        /// </summary>
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
            Header_SizeMo.Header = ResourceManager.GetString("Text_SizeMo");

            if (Job != null)
            {
                LoadJob(Job);
            }
        }

        /// <summary>
        /// SavesDataGrid size changed event
        /// </summary>
        private void SavesDataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                double totalWidth = dataGrid.ActualWidth - SystemParameters.VerticalScrollBarWidth;
                dataGrid.Columns[0].Width = totalWidth * 0.25;  // 25% for "Date"
                dataGrid.Columns[1].Width = totalWidth * 0.25;  // 25% for "Backup name"
                dataGrid.Columns[2].Width = totalWidth * 0.2;   // 20% for "Type"
                dataGrid.Columns[3].Width = totalWidth * 0.3;   // 30% for "Size"
            }
        }

        /// <summary>
        /// Button Submit click event
        /// </summary>
        private async void Button_Submit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = Textbox_BackupName.Text;
                string source = Textbox_SourceDirectory.Text;
                string target = Textbox_TargetDirectory.Text;
                string? type = ((ComboBoxItem)ComboBox_Type.SelectedItem)?.Content?.ToString();

                if (string.IsNullOrWhiteSpace(type))
                {
                    MessageBox.Show(ResourceManager.GetString("Message_Fill"), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var job = new ModelJob(name, source, target, Enum.Parse<BackupTypes>(type));
                if (Job == null)
                {
                    try
                    {
                        await BackupManager.AddBackupJobAsync(job);
                        MessageBox.Show(string.Format(ResourceManager.GetString("Message_AddSuccess") ?? "Success: {0}", job.Name), ResourceManager.GetString("MessageTitle_Success"), MessageBoxButton.OK, MessageBoxImage.Information);

                        // Réinitialiser les champs après ajout
                        Textbox_BackupName.Text = "";
                        Textbox_SourceDirectory.Text = "";
                        Textbox_TargetDirectory.Text = "";
                        ComboBox_Type.SelectedIndex = -1;
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("Message_NameExists"))
                        {
                            MessageBox.Show(ResourceManager.GetString("Message_NameExists"), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            MessageBox.Show(string.Format(ResourceManager.GetString("Error") ?? "Error: {0}", ex.Message), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    try
                    {
                        await BackupManager.UpdateBackupJobAsync(job, Job);
                        MessageBox.Show(string.Format(ResourceManager.GetString("Message_UpdateSuccess") ?? "Update Success: {0}", job.Name), ResourceManager.GetString("MessageTitle_Success"), MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("Message_NameExists"))
                        {
                            MessageBox.Show(ResourceManager.GetString("Message_NameExists"), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            MessageBox.Show(string.Format(ResourceManager.GetString("Error") ?? "Error: {0}", ex.Message), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Button Browse click event
        /// </summary>
        private void BrowseDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string title = button.Name switch
                {
                    "BrowseSource" => ResourceManager.GetString("Title_BrowseSource") ?? "Select the source directory",
                    "BrowseTarget" => ResourceManager.GetString("Title_BrowseTarget") ?? "Select the target directory",
                    _ => "Sélectionner le dossier"
                };

                var dialog = new OpenFileDialog()
                {
                    CheckFileExists = false,
                    CheckPathExists = true,
                    FileName = "Dossier sélectionné",
                    Filter = "Dossiers|*.none",
                    Title = title
                };
                dialog.ShowDialog();

                if (button.Name == "BrowseSource")
                {
                    Textbox_SourceDirectory.Text = System.IO.Path.GetDirectoryName(dialog.FileName);
                }
                else if (button.Name == "BrowseTarget")
                {
                    Textbox_TargetDirectory.Text = System.IO.Path.GetDirectoryName(dialog.FileName);
                }
            }
        }

        /// <summary>
        /// Load the job in the EditPage
        /// </summary>
        private async void LoadJob(ModelJob job)
        {
            Title_Edit.Text = ResourceManager.GetString("Title_Edit") + " - " + job.Name;
            Textbox_BackupName.Text = job.Name;
            Textbox_SourceDirectory.Text = job.SourceDirectory;
            Textbox_TargetDirectory.Text = job.TargetDirectory;
            ComboBox_Type.SelectedIndex = (int)job.Type;
            Title_SavesList.Visibility = Visibility.Visible;
            SavesList.Visibility = Visibility.Visible;
            await DisplaySaves();
        }

        /// <summary>
        /// Display all saves in the EditPage
        /// </summary>
        private Task DisplaySaves()
        {
            return Task.Run(() =>
            {
                // Stock all saves in a list
                SavesEntries = [];

                ObservableCollection<string> SavesName = [];

                if (Job != null && !Directory.Exists(Job.TargetDirectory))
                {
                    Directory.CreateDirectory(Job.TargetDirectory);                                              //create directory if it doesn't exist
                }

                // Go through each folders in the backUpJob target directory
                if (Job != null)
                {
                    foreach (string dir in Directory.GetDirectories(Job.TargetDirectory, "*", SearchOption.TopDirectoryOnly))
                    {
                        string folderName = Path.GetFileName(dir); // Extraire le nom du dossier
                        long size = 0;
                        if (folderName.Contains(Job.Name, StringComparison.OrdinalIgnoreCase)) // Comparaison insensible à la casse
                        {
                            // Get the size of the save
                            size = Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).Sum(file => new FileInfo(file).Length);

                            // Get the type of the save
                            string type = "X";
                            if (folderName.Contains("Diff"))
                            {
                                type = "Differential";
                            }
                            else if (folderName.Contains("Full"))
                            {
                                type = "Full";
                            }

                            // Get the date of creation of the file
                            DirectoryInfo dirInfo = new(dir);

                            SavesEntries.Add(new ModelSave(folderName, type, (size/1024/1024), dirInfo.CreationTime));
                        }
                    }
                }
            });
        }
    }
}
