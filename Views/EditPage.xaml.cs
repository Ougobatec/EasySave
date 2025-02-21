using System.Collections.ObjectModel;
using System.IO;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using EasySave.Enumerations;
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
        private readonly ModelJob? Job = null;      // Used to keep the data on the current job to use it between methods
        private static ResourceManager ResourceManager => BackupManager.GetInstance().resourceManager;
        public ObservableCollection<ModelSave> SavesEntries { get; set; }       // List to store all SaveEntries

        /// <summary>
        /// Constructor of EditPage class
        /// parameter (ModelJob? job) : used to get the infos of the BackUpJob if there's one (when you click on modify on a backUpJob), can be null
        /// </summary>
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

        /// <summary>
        /// Refresh the UI of EditPage
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
            Header_Size.Header = ResourceManager.GetString("Text_Size");
        }
        /// <summary>
        /// Allows to make the grid that contains the save responsive
        /// </summary>
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
        /// <summary>
        /// Load the job by pre-filling the inputs if there's one
        /// </summary>
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
        /// <summary>
        /// Method called when the button submit is clicked, it is used to create or modify a backUpJob
        /// </summary>
        private async void Button_Submit_Click(object sender, RoutedEventArgs e)
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

                var job = new ModelJob(name, source, target, Enum.Parse<BackupTypes>(type));
                //    {
                //    Name = name,
                //    SourceDirectory = source,
                //    TargetDirectory = target,
                //    Type = Enum.Parse<BackupTypes>(type),
                //};)
                if (Job == null)
                {
                    try
                    {
                        await BackupManager.GetInstance().AddBackupJobAsync(job);
                        MessageBox.Show(string.Format(ResourceManager.GetString("Message_AddSuccess"), job.Name), ResourceManager.GetString("MessageTitle_Success"), MessageBoxButton.OK, MessageBoxImage.Information);

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
                            MessageBox.Show(string.Format(ResourceManager.GetString("Error"), ex.Message), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    try
                    {
                        await BackupManager.GetInstance().UpdateBackupJobAsync(job, Job);
                        MessageBox.Show(string.Format(ResourceManager.GetString("Message_UpdateSuccess"), job.Name), ResourceManager.GetString("MessageTitle_Success"), MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("Message_NameExists"))
                        {
                            MessageBox.Show(ResourceManager.GetString("Message_NameExists"), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            MessageBox.Show(string.Format(ResourceManager.GetString("Error"), ex.Message), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
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
        /// Method used to browse directories with file explorer
        /// </summary>
        private void BrowseDirectory_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                if (button.Name == "BrowseSource")
                {
                    var Title = "Sélectionner le dossier de source";
                }
                else if (button.Name == "BrowseTarget")
                {
                    var Title = "Sélectionner le dossier de destination";
                }
                var dialog = new OpenFileDialog()
                {
                    CheckFileExists = false,
                    CheckPathExists = true,
                    FileName = "Dossier sélectionné",
                    Filter = "Dossiers|*.none",
                    Title = Title
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
        /// Method to display saves of the backUpJob
        /// </summary>
        private async void DisplaySaves()
        {
            // Stock all saves in a list
            SavesEntries = new ObservableCollection<ModelSave>();

            ObservableCollection<string> SavesName = new ObservableCollection<string>();

            if (!Directory.Exists(Job.TargetDirectory))
            {
                Directory.CreateDirectory(Job.TargetDirectory);                                              //create directory if it doesn't exist
            }

            // Go through each folders in the backUpJob target directory
            foreach (string dir in Directory.GetDirectories(Job.TargetDirectory, "*", SearchOption.TopDirectoryOnly))
            {
                string folderName = Path.GetFileName(dir); // Extraire le nom du dossier
                long size = 0;
                if (folderName.Contains(Job.Name, StringComparison.OrdinalIgnoreCase)) // Comparaison insensible à la casse
                {
                    // Get the size of the save
                    if (!Directory.Exists(dir))
                    {
                        // if the repertory doesn't exist we set the size to -1
                        size = -1;
                    }
                    else
                    {
                        // Get all files and do a sum their size together
                        foreach (string file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                        {
                            FileInfo fileInfo = new FileInfo(file);
                            size += fileInfo.Length;
                        }
                    }

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
                    DirectoryInfo dirInfo = new DirectoryInfo(dir);

                    SavesEntries.Add(new ModelSave
                    {
                        Name = folderName,
                        Size = size,
                        Type = type,
                        Date = dirInfo.CreationTime,
                    });
                }
            }
        }
    }
}
