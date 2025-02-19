using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
            Header_Logs_Date.Header = ResourceManager.GetString("Header_Logs_Date");
            Header_Logs_Name.Header = ResourceManager.GetString("Header_Logs_Name");
            Header_Logs_Source_Directory.Header = ResourceManager.GetString("Header_Logs_Source_Directory");
            Header_Logs_Target_Directory.Header = ResourceManager.GetString("Header_Logs_Target_Directory");
            Header_Logs_Size.Header = ResourceManager.GetString("Header_Logs_Size");
            Header_Logs_Transfer_Time.Header = ResourceManager.GetString("Header_Logs_Transfer_Time");
            DisplayLogs();
        }

        private void LogsDataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                double totalWidth = dataGrid.ActualWidth - SystemParameters.VerticalScrollBarWidth; // Largeur disponible
                dataGrid.Columns[0].Width = totalWidth * 0.1; // 10% pour "Horodatage"
                dataGrid.Columns[1].Width = totalWidth * 0.1; // 10% pour "Nom sauvegarde"
                dataGrid.Columns[2].Width = totalWidth * 0.3; // 30% pour "Emplacement source"
                dataGrid.Columns[3].Width = totalWidth * 0.3; // 30% pour "Emplacement cible"
                dataGrid.Columns[4].Width = totalWidth * 0.1; // 10% pour "Taille"
                dataGrid.Columns[5].Width = totalWidth * 0.1; // 10% pour "Temps de transfert"
            }
        }

        private async void DisplayLogs()
        {
            LogEntries = new ObservableCollection<ModelLog>(await Logger<ModelLog>.GetInstance().GetLogs());
        }
    }
}
