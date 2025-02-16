using System.Windows;
using System.Windows.Controls;

namespace EasySave
{
    /// <summary>
    /// Logique d'interaction pour Page1.xaml
    /// </summary>
    public partial class Logs : Page
    {
        public Logs()
        {
            InitializeComponent();
        }

        private void LogsListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (LogsListView.View is GridView gridView)
            {
                double totalWidth = LogsListView.ActualWidth - SystemParameters.VerticalScrollBarWidth; // Largeur disponible
                double proportion1 = 0.1;  // 10% pour "Horodatage"
                double proportion2 = 0.2;  // 20% pour "Nom sauvegarde"
                double proportion3 = 0.25;  // 25% pour "Emplacement source"
                double proportion4 = 0.25;  // 25% pour "Emplacement cible"

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
    }
}
