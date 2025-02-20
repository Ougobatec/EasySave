using System.Globalization;
using System.Resources;
using System.Windows.Controls;

namespace EasySave.Views
{
    /// <summary>
    /// Logique d'interaction pour SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private static ResourceManager ResourceManager => BackupManager.GetInstance().resourceManager;

        public SettingsPage()
        {
            InitializeComponent();
            DataContext = this;
            Refresh();
        }

        private void Refresh()
        {
            MainWindow.GetInstance().Refresh();
            Title_Settings.Text = ResourceManager.GetString("Title_Settings");
            Text_Language.Text = ResourceManager.GetString("Text_Language");
            Text_LogFormat.Text = ResourceManager.GetString("Text_LogFormat");
            ComboBox_Language.Text = BackupManager.GetInstance().JsonConfig.Language.ToString();
            ComboBox_LogFormat.Text = BackupManager.GetInstance().JsonConfig.LogFormat.ToString();
        }

        private void Setting_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                if (comboBox.Name == "ComboBox_Language")
                {
                    BackupManager.GetInstance().ChangeSettingsAsync((comboBox.SelectedItem as ComboBoxItem)?.Content.ToString(), null);
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(BackupManager.GetInstance().JsonConfig.Language.ToString());
                }
                else if (comboBox.Name == "ComboBox_LogFormat")
                {
                    BackupManager.GetInstance().ChangeSettingsAsync(null, (comboBox.SelectedItem as ComboBoxItem)?.Content.ToString());
                }
            }
            Refresh();
        }
    }
}
