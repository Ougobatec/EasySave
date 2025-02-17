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

        private void LogsListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (LogsListView.View is GridView gridView)
            {
                double totalWidth = LogsListView.ActualWidth - SystemParameters.VerticalScrollBarWidth; // Largeur disponible
                double proportion1 = 0.1;  // 10% pour "Horodatage"
                double proportion2 = 0.1;  // 20% pour "Nom sauvegarde"
                double proportion3 = 0.3;  // 25% pour "Emplacement source"
                double proportion4 = 0.3;  // 25% pour "Emplacement cible"
                double proportion5 = 0.1;   // 10% pour "Modifier"
                double proportion6 = 0.1;   // 10% pour "Etat"


                gridView.Columns[0].Width = totalWidth * proportion1;
                gridView.Columns[1].Width = totalWidth * proportion2;
                gridView.Columns[2].Width = totalWidth * proportion3;
                gridView.Columns[3].Width = totalWidth * proportion4;
                gridView.Columns[4].Width = totalWidth * proportion5;
                gridView.Columns[5].Width = totalWidth * proportion6;
            }
        }

        private async void DisplayLogs()
        {
            LogEntries = new ObservableCollection<ModelLog>(await Logger<ModelLog>.GetInstance().GetLogs());
        }
    }
}
