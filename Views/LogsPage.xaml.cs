using System.Collections.ObjectModel;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using EasySave.Models;
using Logger;

namespace EasySave.Views
{
    /// <summary>
    /// Interaction logic for LogsPage.xaml
    /// </summary>
    public partial class LogsPage : Page
    {
        private static BackupManager BackupManager => BackupManager.GetInstance();          // Backup manager instance
        private static ResourceManager ResourceManager => BackupManager.resourceManager;    // Resource manager instance
        public ObservableCollection<ModelLog> LogEntries { get; set; }                      // List to get all logs

        /// <summary>
        /// LogsPage constructor to initialize the page and display logs
        /// </summary>
        public LogsPage()
        {
            InitializeComponent();
            DataContext = this;
            Refresh();
        }

        /// <summary>
        /// Refresh the LogsPage content
        /// </summary>
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

        /// <summary>
        /// LogsDataGrid size changed event
        /// </summary>
        private void LogsDataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                double totalWidth = dataGrid.ActualWidth - SystemParameters.VerticalScrollBarWidth;
                dataGrid.Columns[0].Width = totalWidth * 0.1;   // 10% for "Date"
                dataGrid.Columns[1].Width = totalWidth * 0.14;  // 10% for "Backup name"
                dataGrid.Columns[2].Width = totalWidth * 0.24;  // 30% for "Source directory"
                dataGrid.Columns[3].Width = totalWidth * 0.24;  // 30% for "Target directory"
                dataGrid.Columns[4].Width = totalWidth * 0.08;  // 10% for "Size"
                dataGrid.Columns[5].Width = totalWidth * 0.1;   // 10% for "Encryption time"
                dataGrid.Columns[6].Width = totalWidth * 0.1;   // 10% for "Transfer time"
            }
        }

        /// <summary>
        /// Display all logs
        /// </summary>
        private void DisplayLogs()
        {
            LogEntries = new ObservableCollection<ModelLog>(Logger<ModelLog>.GetInstance().GetLogs());
        }
    }
}
