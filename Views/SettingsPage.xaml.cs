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
        private static BackupManager BackupManager => BackupManager.GetInstance();              // BackupManager instance
        private static ResourceManager ResourceManager => BackupManager.resourceManager;        // Resource manager instance
        public ObservableCollection<string> AvailablePriorityExtensions { get; set; } = [];     // List for the available extensions for priority exentions
        public ObservableCollection<string> AvailableEncryptedExtensions { get; set; } = [];    // List for the available extensions for encrypted exentions
        public ObservableCollection<string> SelectedPriorityExtensions { get; set; } = [];      // List for the selected extensions for priority exentions
        public ObservableCollection<string> SelectedEncryptedExtensions { get; set; } = [];     // List for the selected extensions for encrypted exentions
        public ObservableCollection<string> Extensions { get; set; } = [];                      // List to get all extensions
        public ObservableCollection<string> BusinessSoftwares { get; set; } = [];               // List to of all business softwares
        public ObservableCollection<ModelJob> BackupJobs { get; set; } = [];                    // List to get all backupJobs

        /// <summary>
        /// SettingsPage constructor to initialize the page and display settings
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();
            DataContext = this;
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
            Add_BusinessSoftwareButton.Content = ResourceManager.GetString("Add_BusinessSoftware");
            Remove_BusinessSoftwareButton.Content = ResourceManager.GetString("Remove_BusinessSoftware");
            ComboBox_Language.Text = BackupManager.JsonConfig.Language.ToString();
            ComboBox_LogFormat.Text = BackupManager.JsonConfig.LogFormat.ToString();
            TextBox_LimitSize.Text = BackupManager.JsonConfig.LimitSizeFile.ToString();

            Get_all_extension();
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
            foreach (string el in BackupManager.JsonConfig.BusinessSoftwares)
            {
                BusinessSoftwares.Add(el);
            }
        }

        /// <summary>
        /// Setting changed event
        /// </summary>
        private async void Setting_Changed(object sender, SelectionChangedEventArgs? e = null)
        {
            if (sender is ComboBox comboBox)
            {
                if (comboBox.Name == "ComboBox_Language")
                {
                    if (comboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Content != null)
                    {
                        await BackupManager.ChangeSettingsAsync("language", selectedItem.Content.ToString() ?? string.Empty);
                        Thread.CurrentThread.CurrentUICulture = new CultureInfo(BackupManager.JsonConfig.Language.ToString());
                    }
                }
                else if (comboBox.Name == "ComboBox_LogFormat")
                {
                    if (comboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Content != null)
                    {
                        await BackupManager.ChangeSettingsAsync("logFormat", selectedItem.Content.ToString() ?? string.Empty);
                    }
                }
            }
            if (sender is Button button)
            {
                List<string> LitsExtensions = [];
                if (button.Name.Contains("Priority"))
                {
                    foreach (var item in SelectedExtensionsListBox.Items)
                    {
                        if (item is string extension)
                        {
                            LitsExtensions.Add(extension);
                        }
                    }
                    await BackupManager.ChangeSettingsAsync("PriorityFiles", string.Empty, LitsExtensions);
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
                    await BackupManager.ChangeSettingsAsync("EncryptedFiles", string.Empty, LitsExtensions);
                }
                else if (button.Name.Contains("Add_BusinessSoftwareButton"))
                {
                    await BackupManager.ChangeSettingsAsync("Add_BusinessSoftware", TextBox_BusinessSoftwares.Text);
                    // We remove the text in the input after adding a business software
                    TextBox_BusinessSoftwares.Text = "";
                }
                else if (button.Name.Contains("Remove_BusinessSoftwareButton"))
                {
                    if (!string.IsNullOrEmpty(TextBox_BusinessSoftwares.Text))
                    {
                        await BackupManager.ChangeSettingsAsync("Remove_BusinessSoftware", TextBox_BusinessSoftwares.Text);
                    }
                    if (BusinessSoftwaresListBox.SelectedItem != null)
                    {
                        await BackupManager.ChangeSettingsAsync("Remove_BusinessSoftware", BusinessSoftwaresListBox.SelectedItem?.ToString() ?? string.Empty);
                        // We remove the selected item from the list
                        if (BusinessSoftwaresListBox.SelectedItem != null)
                        {
                            BusinessSoftwares.Remove(BusinessSoftwaresListBox.SelectedItem?.ToString() ?? string.Empty);
                        }
                    }
                    else if (!string.IsNullOrEmpty(TextBox_BusinessSoftwares.Text) && BusinessSoftwaresListBox.SelectedItem != null)
                    {
                        throw new Exception("Message_SelectionBusinessSoftware");
                    }
                }
            }
            Refresh();
        }

        /// <summary>
        /// Add a business software to the list
        /// </summary>
        private void Add_BusinessSoftware(object sender, RoutedEventArgs e)
        {
            if (TextBox_BusinessSoftwares.Text != "")
            {
                bool already_in_list = false;
                foreach (var el in BusinessSoftwares)
                {
                    if (TextBox_BusinessSoftwares.Text == el.ToString())
                    {
                        already_in_list = true;
                    }
                }
                if (already_in_list)
                {
                    //throw new Exception("Message_AlreadyInListBusinessSoftware");
                }
                else
                {
                    BusinessSoftwares.Add(TextBox_BusinessSoftwares.Text);
                }
                Setting_Changed(sender);
            }
        }

        /// <summary>
        /// Remove a business software to the list
        /// </summary>
        private void Remove_BusinessSoftware(object sender, RoutedEventArgs e)
        {
            if (TextBox_BusinessSoftwares.Text != "")
            {
                BusinessSoftwares.Remove(TextBox_BusinessSoftwares.Text);
            }
            Setting_Changed(sender);
        }

        /// <summary>
        /// Move one element to the selectionned list
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
        /// Move all elements to the selected list
        /// </summary>
        private void MoveAllToSelected(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
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
        }

        /// <summary>
        /// Move one element to the available list
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
        /// Move all elements to the available list
        /// </summary>
        private void MoveAllToAvailable(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
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
        }

        /// <summary>
        /// Limit size file text changed event
        /// </summary>
        private async void Limit_size_fileTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Name == "TextBox_LimitSize")
                {
                    if (int.TryParse(textBox.Text, out int limitSize))
                    {
                        await BackupManager.ChangeSettingsAsync("limitSizeFile", limitSize.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Get all extensions from the backup jobs
        /// </summary>
        private void Get_all_extension()
        {
            BackupJobs = [.. BackupManager.JsonConfig.BackupJobs];

            // Creation of a hashset to avoid duplicates
            HashSet<string> fileExtensions = [];

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
            Extensions = [.. fileExtensions];

            foreach (string el in Extensions)
            {
                AvailablePriorityExtensions.Add(el);
                AvailableEncryptedExtensions.Add(el);
            }
        }
    }
}
