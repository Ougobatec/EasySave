using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace EasySave
{
    /// <summary>
    /// Logique d'interaction pour Page1.xaml
    /// </summary>
    public partial class Settings : Page
    {
        private ResourceManager ResourceManager => BackupManager.GetInstance().resourceManager;
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
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(BackupManager.GetInstance().Config.Language.ToString());

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Mise à jour des textes sur le thread principal
                        MainWindow.GetInstance().RefreshUI();
                        // Language Changes
                        Type_logs.Text = ResourceManager.GetString("Type_logs");
                        Language.Text = ResourceManager.GetString("Language");
                        Title_Settings.Text = ResourceManager.GetString("Title_Settings");
                    });
                }
             }
        }
    }
}
