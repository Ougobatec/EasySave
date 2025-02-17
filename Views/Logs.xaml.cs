using EasySave.Models;
using Logger;
using System.Collections.ObjectModel;
using System.Resources;
using System.Windows;
using System.Windows.Controls;

namespace EasySave
{
    /// <summary>
    /// Logique d'interaction pour Page1.xaml
    /// </summary>
    public partial class Logs : Page
    {
        private ResourceManager ResourceManager => BackupManager.GetInstance().resourceManager;
        public ObservableCollection<ModelLog> LogEntries { get; set; }

        public Logs()
        {
            InitializeComponent();
            DataContext = this;
            DisplayLogs();
            // Language changes
            Header_Logs_Date.Header = ResourceManager.GetString("Header_Logs_Date");
            Header_Logs_Name.Header = ResourceManager.GetString("Header_Logs_Name");
            Header_Logs_Source_Directory.Header = ResourceManager.GetString("Header_Logs_Source_Directory");
            Header_Logs_Target_Directory.Header = ResourceManager.GetString("Header_Logs_Target_Directory");
            Header_Logs_Size.Header = ResourceManager.GetString("Header_Logs_Size");
            Header_Logs_Transfer_Time.Header = ResourceManager.GetString("Header_Logs_Transfer_Time");
        }

        private void LogsDataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                double totalWidth = dataGrid.ActualWidth - SystemParameters.VerticalScrollBarWidth; // Largeur disponible
                double proportion1 = 0.1;  // 10% pour "Horodatage"
                double proportion2 = 0.1;  // 10% pour "Nom sauvegarde"
                double proportion3 = 0.3;  // 30% pour "Emplacement source"
                double proportion4 = 0.3;  // 30% pour "Emplacement cible"
                double proportion5 = 0.1;  // 10% pour "Taille"
                double proportion6 = 0.1;  // 10% pour "Temps de transfert"

                dataGrid.Columns[0].Width = totalWidth * proportion1;
                dataGrid.Columns[1].Width = totalWidth * proportion2;
                dataGrid.Columns[2].Width = totalWidth * proportion3;
                dataGrid.Columns[3].Width = totalWidth * proportion4;
                dataGrid.Columns[4].Width = totalWidth * proportion5;
                dataGrid.Columns[5].Width = totalWidth * proportion6;
            }
        }

        private async void DisplayLogs()
        {
            LogEntries = new ObservableCollection<ModelLog>(await Logger<ModelLog>.GetInstance().GetLogs());
        }
    }
}
