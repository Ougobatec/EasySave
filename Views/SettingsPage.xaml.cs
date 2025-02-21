﻿using EasySave.Models;
using Logger;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace EasySave.Views
{
    /// <summary>
    /// Logique d'interaction pour SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private static ResourceManager ResourceManager => BackupManager.GetInstance().resourceManager;
        public ObservableCollection<string> AvailableExtensions { get; set; } = new ObservableCollection<string> { };       // List for the available extensions for priority exentions
        public ObservableCollection<string> AvailableEncryptedExtensions { get; set; } = new ObservableCollection<string> { };      // List for the available extensions for encrypted exentions
        public ObservableCollection<string> SelectedExtensions { get; set; } = new ObservableCollection<string>();      // List for the selected extensions for priority exentions
        public ObservableCollection<string> SelectedEncryptedExtensions { get; set; } = new ObservableCollection<string>();     // List for the selected extensions for encrypted exentions
        public ObservableCollection<ModelJob> BackupJobs { get; set; }      // List to get all backupJobs 
        public ObservableCollection<string> Extensions { get; set; } = new ObservableCollection<string>();     // List to store all extensions used in backUps

        public SettingsPage()
        {
            InitializeComponent();
            // Liaison avec le DataContext
            DataContext = this;
            Get_all_extension();
            Refresh();
        }

        /// <summary>
        /// Refresh the UI of SettingsPage
        /// </summary>
        private void Refresh()
        {
            MainWindow.GetInstance().Refresh();
            Title_Settings.Text = ResourceManager.GetString("Title_Settings");
            Text_Language.Text = ResourceManager.GetString("Text_Language");
            Text_LogFormat.Text = ResourceManager.GetString("Text_LogFormat");
            Title_Priority_extension.Text = ResourceManager.GetString("Title_Priority_extension");
            Title_Encrypted_extension.Text = ResourceManager.GetString("Title_Encrypted_extension");
            ComboBox_Language.Text = BackupManager.GetInstance().JsonConfig.Language.ToString();
            ComboBox_LogFormat.Text = BackupManager.GetInstance().JsonConfig.LogFormat.ToString();
            Limit_size_fileTextBox.Text = BackupManager.GetInstance().JsonConfig.limitSizeFile.ToString();
        }

        /// <summary>
        /// Method to change parameters
        /// </summary>
        private void Setting_Changed(object sender, SelectionChangedEventArgs e)
        {
            // Parameters with ComboBox
            if (sender is ComboBox comboBox)
            {
                if (comboBox.Name == "ComboBox_Language")
                {
                    BackupManager.GetInstance().ChangeSettingsAsync("language", (comboBox.SelectedItem as ComboBoxItem)?.Content.ToString());
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(BackupManager.GetInstance().JsonConfig.Language.ToString());
                }
                else if (comboBox.Name == "ComboBox_LogFormat")
                {
                    BackupManager.GetInstance().ChangeSettingsAsync("logFormat", (comboBox.SelectedItem as ComboBoxItem)?.Content.ToString());
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
                AvailableExtensions.Remove(selectedExtension);
                SelectedExtensions.Add(selectedExtension);
            }
            else if (AvailableEncryptExtensionsListBox.SelectedItem is string SelectedEncryptedExtension)
            {
                AvailableEncryptedExtensions.Remove(SelectedEncryptedExtension);
                SelectedEncryptedExtensions.Add(SelectedEncryptedExtension);
            }
        }

        /// <summary>
        /// move all elements to the selectionned list
        /// </summary>
        private void MoveAllToSelected(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Name == "MoveAllToSelectedPriority")
            {
                while (AvailableExtensions.Count > 0)
                {
                    string ext = AvailableExtensions[0];
                    AvailableExtensions.RemoveAt(0);
                    SelectedExtensions.Add(ext);
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
        }

        /// <summary>
        /// move one element to the available list
        /// </summary>
        private void MoveToAvailable(object sender, RoutedEventArgs e)
        {
            if (SelectedExtensionsListBox.SelectedItem is string selectedExtension)
            {
                SelectedExtensions.Remove(selectedExtension);
                AvailableExtensions.Add(selectedExtension);
            }
            else if (SelectedEncryptExtensionsListBox.SelectedItem is string SelectedEncryptedExtension)
            {
                SelectedEncryptedExtensions.Remove(SelectedEncryptedExtension);
                AvailableEncryptedExtensions.Add(SelectedEncryptedExtension);
            }
        }

        /// <summary>
        /// Move all elements to the avaialble list
        /// </summary>
        private void MoveAllToAvailable(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Name == "MoveAllToAvailablePriority")
            {
                while (SelectedExtensions.Count > 0)
                {
                    string ext = SelectedExtensions[0];
                    SelectedExtensions.RemoveAt(0);
                    AvailableExtensions.Add(ext);
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
        }
        private void Limit_size_fileTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Name == "Limit_size_fileTextBox")
                {
                    BackupManager.GetInstance().ChangeSettingsAsync("limitSizeFile", textBox.Text.ToString());
                }
            }
        }
        /// <summary>
        /// allow to get all extensions used in the saves files and list them in available lists
        /// </summary>
        private async void Get_all_extension()
        {
            BackupJobs = new ObservableCollection<ModelJob>(BackupManager.GetInstance().JsonConfig.BackupJobs);

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
                AvailableExtensions.Add(el);
                AvailableEncryptedExtensions.Add(el);
            }
        }
    }
}
