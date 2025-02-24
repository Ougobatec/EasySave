using System.Collections.ObjectModel;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using EasySave.Models;
using Logger;

namespace EasySave.Views
{
    /// <summary>
    /// Logique d'interaction pour LogsPage.xaml
    /// </summary>
    public partial class LogsPage : Page
    {
        private static ResourceManager ResourceManager => BackupManager.GetInstance().resourceManager;
        public ObservableCollection<ModelLog> LogEntries { get; set; }

        public LogsPage()
        {
            InitializeComponent();
            DataContext = this;
            Refresh();
        }

        private void Refresh()
        {
            MainWindow.GetInstance().Refresh();
            Title_Logs.Text = ResourceManager.GetString("Title_Logs");
            Header_Date.Header = ResourceManager.GetString("Text_Date");
            Header_BackupName.Header = ResourceManager.GetString("Text_BackupName");
            Header_SourceDirectory.Header = ResourceManager.GetString("Text_SourceDirectory");
            Header_TargetDirectory.Header = ResourceManager.GetString("Text_TargetDirectory");
            Header_Size.Header = ResourceManager.GetString("Text_Size");
            Header_EncryptionTime.Header = ResourceManager.GetString("Text_EncryptionTime");
            Header_TransferTime.Header = ResourceManager.GetString("Text_TransferTime");
            DisplayLogs();
        }

        private void LogsDataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                double totalWidth = dataGrid.ActualWidth - SystemParameters.VerticalScrollBarWidth;
                dataGrid.Columns[0].Width = totalWidth * 0.1;   // 10% pour "Horodatage"
                dataGrid.Columns[1].Width = totalWidth * 0.14;  // 10% pour "Nom sauvegarde"
                dataGrid.Columns[2].Width = totalWidth * 0.24;  // 30% pour "Emplacement source"
                dataGrid.Columns[3].Width = totalWidth * 0.24;  // 30% pour "Emplacement cible"
                dataGrid.Columns[4].Width = totalWidth * 0.08;  // 10% pour "Taille"
                dataGrid.Columns[5].Width = totalWidth * 0.1;   // 10% pour "Temps de cryptage"
                dataGrid.Columns[6].Width = totalWidth * 0.1;   // 10% pour "Temps de transfert"
            }
        }

        private void DisplayLogs()
        {
            LogEntries = new ObservableCollection<ModelLog>(Logger<ModelLog>.GetInstance().GetLogs());
        }
    }
}
