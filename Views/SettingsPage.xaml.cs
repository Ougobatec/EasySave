using EasySave.Models;
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
        public ObservableCollection<string> AvailableExtensions { get; set; } = new ObservableCollection<string> { };
        public ObservableCollection<string> AvailableEncryptedExtensions { get; set; } = new ObservableCollection<string> { };
        public ObservableCollection<string> SelectedExtensions { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> SelectedEncryptedExtensions { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<ModelLog> LogEntries { get; set; }
        public ObservableCollection<string> PathEntries { get; set; } = new ObservableCollection<string>();

        public SettingsPage()
        {
            InitializeComponent();
            // Liaison avec le DataContext
            DataContext = this;
            Get_all_extension();
            Refresh();
        }

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

        // Déplacer un élément vers la liste sélectionnée
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

        // Déplacer tous les éléments vers la liste sélectionnée
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

        // Déplacer un élément vers la liste disponible
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

        // Déplacer tous les éléments vers la liste disponible
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
        private async void Get_all_extension()
        {
            //LogEntries = new ObservableCollection<ModelLog>(await Logger<ModelLog>.GetInstance().GetLogs());

            //foreach (ModelLog el in LogEntries.ToList())
            //{
            //    // Extraction des extensions et suppression des doublons
            //    HashSet<string> fileExtensions = new HashSet<string>(
            //        PathEntries.Select(path => Path.GetExtension(path).ToLower())
            //                 .Where(ext => !string.IsNullOrEmpty(ext))
            //    );
            //}

            LogEntries = new ObservableCollection<ModelLog>(await Logger<ModelLog>.GetInstance().GetLogs());

            // Création d'un HashSet pour éviter les doublons
            HashSet<string> fileExtensions = new HashSet<string>();

            foreach (ModelLog el in LogEntries)
            {
                // Vérifie si destination n'est pas vide ou null
                if (!string.IsNullOrEmpty(el.Destination))
                {
                    string ext = Path.GetExtension(el.Destination).ToLower();
                    if (!string.IsNullOrEmpty(ext))
                    {
                        fileExtensions.Add(ext);
                    }
                }
            }

            // Mettre à jour la liste PathEntries sans doublons
            PathEntries = new ObservableCollection<string>(fileExtensions);

            foreach (string el in PathEntries)
            {
                AvailableExtensions.Add(el);
                AvailableEncryptedExtensions.Add(el);
            }
        }
    }
}
