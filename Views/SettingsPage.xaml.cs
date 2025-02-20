using System.Collections.ObjectModel;
using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Controls;

namespace EasySave.Views
{
    /// <summary>
    /// Logique d'interaction pour SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private static ResourceManager ResourceManager => BackupManager.GetInstance().resourceManager;
        public ObservableCollection<string> AvailableExtensions { get; set; } = [".txt", ".jpg", ".png"];
        public ObservableCollection<string> SelectedExtensions { get; set; } = [];

        public SettingsPage()
        {
            InitializeComponent();
            // Liaison avec le DataContext
            DataContext = this;
            Refresh();
        }

        private void Refresh()
        {
            MainWindow.GetInstance().Refresh();
            Title_Settings.Text = ResourceManager.GetString("Title_Settings");
            Text_Language.Text = ResourceManager.GetString("Text_Language");
            Text_LogFormat.Text = ResourceManager.GetString("Text_LogFormat");
            Title_ExtensionsSettings.Text = ResourceManager.GetString("Title_ExtensionsSettings");
            ComboBox_Language.Text = BackupManager.GetInstance().JsonConfig.Language.ToString();
            ComboBox_LogFormat.Text = BackupManager.GetInstance().JsonConfig.LogFormat.ToString();
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
            // Parameters with textBox
            else if (sender is TextBox textBox)
            {
                if (textBox.Name == "Limit_size_fileTextBox")
                {
                    BackupManager.GetInstance().ChangeSettingsAsync("limitSizeFile", textBox.Text.ToString());
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
        }

        // Déplacer tous les éléments vers la liste sélectionnée
        private void MoveAllToSelected(object sender, RoutedEventArgs e)
        {
            while (AvailableExtensions.Count > 0)
            {
                string ext = AvailableExtensions[0];
                AvailableExtensions.RemoveAt(0);
                SelectedExtensions.Add(ext);
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
        }

        // Déplacer tous les éléments vers la liste disponible
        private void MoveAllToAvailable(object sender, RoutedEventArgs e)
        {
            while (SelectedExtensions.Count > 0)
            {
                string ext = SelectedExtensions[0];
                SelectedExtensions.RemoveAt(0);
                AvailableExtensions.Add(ext);
            }
        }
    }
}
