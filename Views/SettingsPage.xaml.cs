using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using EasySave.Models;

namespace EasySave.Views
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private static BackupManager BackupManager => BackupManager.GetInstance();                                              // BackupManager instance
        private static ResourceManager ResourceManager => BackupManager.resourceManager;                                        // Resource manager instance
        public ObservableCollection<string> AvailablePriorityExtensions { get; set; } = new ObservableCollection<string>();     // List for the available extensions for priority exentions
        public ObservableCollection<string> AvailableEncryptedExtensions { get; set; } = new ObservableCollection<string>();    // List for the available extensions for encrypted exentions
        public ObservableCollection<string> SelectedPriorityExtensions { get; set; } = new ObservableCollection<string>();      // List for the selected extensions for priority exentions
        public ObservableCollection<string> SelectedEncryptedExtensions { get; set; } = new ObservableCollection<string>();     // List for the selected extensions for encrypted exentions
        public ObservableCollection<ModelJob> BackupJobs { get; set; }                                                          // List to get all backupJobs 
        public ObservableCollection<string> Extensions { get; set; } = new ObservableCollection<string>();                      // List to get all extensions

        /// <summary>
        /// SettingsPage constructor to initialize the page and display settings
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();
            // Liaison avec le DataContext
            DataContext = this;
            Get_all_extension();
            //SelectedExtensionsListBox.ItemsSource = BackupManager.GetInstance().JsonConfig.PriorityExtensions;
            foreach (string el in BackupManager.JsonConfig.PriorityExtensions)
            {
                AvailablePriorityExtensions.Remove(el);
                SelectedPriorityExtensions.Add(el);
            }
            foreach (string el in BackupManager.JsonConfig.EncryptedExtensions)
            {
                AvailableEncryptedExtensions.Remove(el);
                SelectedEncryptedExtensions.Add(el);
            }
            Refresh();
        }

        /// <summary>
        /// Refresh the SettingsPage content
        /// </summary>
        private void Refresh()
        {
            MainWindow.GetInstance().Refresh();
            Title_Settings.Text = ResourceManager.GetString("Title_Settings");
            Title_PriorityExtension.Text = ResourceManager.GetString("Title_PriorityExtension");
            Title_EncryptedExtension.Text = ResourceManager.GetString("Title_EncryptedExtension");
            Text_Language.Text = ResourceManager.GetString("Text_Language");
            Text_LogFormat.Text = ResourceManager.GetString("Text_LogFormat");
            Text_LimitSize.Text = ResourceManager.GetString("Text_LimitSize");
            ComboBox_Language.Text = BackupManager.JsonConfig.Language.ToString();
            ComboBox_LogFormat.Text = BackupManager.JsonConfig.LogFormat.ToString();
            TextBox_LimitSize.Text = BackupManager.JsonConfig.LimitSizeFile.ToString();
        }

        /// <summary>
        /// Method to change parameters
        /// </summary>
        private void Setting_Changed(object sender, SelectionChangedEventArgs? e = null)
        {
            // Parameters with ComboBox
            if (sender is ComboBox comboBox)
            {
                if (comboBox.Name == "ComboBox_Language")
                {
                    BackupManager.ChangeSettingsAsync("language", (comboBox.SelectedItem as ComboBoxItem)?.Content.ToString());
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(BackupManager.JsonConfig.Language.ToString());
                }
                else if (comboBox.Name == "ComboBox_LogFormat")
                {
                    BackupManager.ChangeSettingsAsync("logFormat", (comboBox.SelectedItem as ComboBoxItem)?.Content.ToString());
                }
            }
            if (sender is Button button)
            {
                List<string> LitsExtensions = new List<string>();
                if (button.Name.Contains("Priority"))
                {
                    foreach (var item in SelectedExtensionsListBox.Items)
                    {
                        if (item is string extension)
                        {
                            LitsExtensions.Add(extension);
                        }
                    }
                    BackupManager.ChangeSettingsAsync("PriorityFiles", null, LitsExtensions);
                }
                else if (button.Name.Contains("Encrypted"))
                {
                    foreach (var item in SelectedEncryptExtensionsListBox.Items)
                    {
                        if (item is string extension)
                        {
                            LitsExtensions.Add(extension);
                        }
                    }
                    BackupManager.ChangeSettingsAsync("EncryptedFiles", null, LitsExtensions);
                }
            }
            Refresh();
        }

        /// <summary>
        /// move one elements to the selectionned list
        /// </summary>
        private void MoveToSelected(object sender, RoutedEventArgs e)
        {
            if (AvailableExtensionsListBox.SelectedItem is string selectedExtension)
            {
                AvailablePriorityExtensions.Remove(selectedExtension);
                SelectedPriorityExtensions.Add(selectedExtension);
            }
            else if (AvailableEncryptExtensionsListBox.SelectedItem is string SelectedEncryptedExtension)
            {
                AvailableEncryptedExtensions.Remove(SelectedEncryptedExtension);
                SelectedEncryptedExtensions.Add(SelectedEncryptedExtension);
            }
            Setting_Changed(sender);
        }

        /// <summary>
        /// move all elements to the selectionned list
        /// </summary>
        private void MoveAllToSelected(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Name == "MoveAllToSelectedPriority")
            {
                while (AvailablePriorityExtensions.Count > 0)
                {
                    string ext = AvailablePriorityExtensions[0];
                    AvailablePriorityExtensions.RemoveAt(0);
                    SelectedPriorityExtensions.Add(ext);
                }
            }
            else if (button.Name == "MoveAllToSelectedEncrypted")
            {
                while (AvailableEncryptedExtensions.Count > 0)
                {
                    string ext = AvailableEncryptedExtensions[0];
                    AvailableEncryptedExtensions.RemoveAt(0);
                    SelectedEncryptedExtensions.Add(ext);
                }
            }
            Setting_Changed(sender);
        }

        /// <summary>
        /// move one element to the available list
        /// </summary>
        private void MoveToAvailable(object sender, RoutedEventArgs e)
        {
            if (SelectedExtensionsListBox.SelectedItem is string selectedExtension)
            {
                SelectedPriorityExtensions.Remove(selectedExtension);
                AvailablePriorityExtensions.Add(selectedExtension);
            }
            else if (SelectedEncryptExtensionsListBox.SelectedItem is string SelectedEncryptedExtension)
            {
                SelectedEncryptedExtensions.Remove(SelectedEncryptedExtension);
                AvailableEncryptedExtensions.Add(SelectedEncryptedExtension);
            }
            Setting_Changed(sender);
        }

        /// <summary>
        /// Move all elements to the avaialble list
        /// </summary>
        private void MoveAllToAvailable(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Name == "MoveAllToAvailablePriority")
            {
                while (SelectedPriorityExtensions.Count > 0)
                {
                    string ext = SelectedPriorityExtensions[0];
                    SelectedPriorityExtensions.RemoveAt(0);
                    AvailablePriorityExtensions.Add(ext);
                }
            }
            else if (button.Name == "MoveAllToAvailableEncrypted")
            {
                while (SelectedEncryptedExtensions.Count > 0)
                {
                    string ext = SelectedEncryptedExtensions[0];
                    SelectedEncryptedExtensions.RemoveAt(0);
                    AvailableEncryptedExtensions.Add(ext);
                }
            }
            Setting_Changed(sender);
        }
        private void Limit_size_fileTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Name == "Limit_size_fileTextBox")
                {
                    BackupManager.ChangeSettingsAsync("limitSizeFile", textBox.Text.ToString());
                }
            }
        }
        /// <summary>
        /// allow to get all extensions used in the saves files and list them in available lists
        /// </summary>
        private async void Get_all_extension()
        {
            BackupJobs = new ObservableCollection<ModelJob>(BackupManager.JsonConfig.BackupJobs);

            // Creation of a hashset to avoid duplicates
            HashSet<string> fileExtensions = new HashSet<string>();

            foreach (ModelJob job in BackupJobs)
            {
                if (Directory.Exists(job.TargetDirectory)) // Verify if the folder exist
                {
                    foreach (string file in Directory.GetFiles(job.TargetDirectory, "*.*", SearchOption.AllDirectories))
                    {
                        string extension = Path.GetExtension(file).ToLower();
                        if (!string.IsNullOrEmpty(extension))
                        {
                            fileExtensions.Add(extension);
                        }
                    }
                }
            }

            // Add the extensions to the list
            Extensions = new ObservableCollection<string>(fileExtensions);

            foreach (string el in Extensions)
            {
                AvailablePriorityExtensions.Add(el);
                AvailableEncryptedExtensions.Add(el);
            }
        }
    }
}
