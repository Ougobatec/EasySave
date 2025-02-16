using System.Windows;
using System.Windows.Controls;

namespace EasySave
{
    /// <summary>
    /// Logique d'interaction pour Page1.xaml
    /// </summary>
    public partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Pre-fill all settings using config file
        /// </summary>
        public void Settings_Loaded()
        {
            // Charger les paramètres après l'affichage de l'interface
            LanguageComboBox.Text = BackupManager.GetInstance().Config.Language.ToString();
            TypeLogsComboBox.Text = BackupManager.GetInstance().Config.LogFormat.ToString();
        }
        /// <summary>
        /// Change a setting in config file when changed
        /// </summary>
        private void Setting_Changed(object sender, SelectionChangedEventArgs e)
        {
            ComboBox button = sender as ComboBox;
            if (button != null) {
                if (button.Name == "TypeLogsComboBox")
                {
                    BackupManager.GetInstance().ChangeSettingsAsync(null, (button.SelectedItem as ComboBoxItem)?.Content.ToString());
                }
                if (button.Name == "LanguageComboBox")
                {
                    BackupManager.GetInstance().ChangeSettingsAsync((button.SelectedItem as ComboBoxItem)?.Content.ToString(), null);
                }
             }
        }
    }
}
