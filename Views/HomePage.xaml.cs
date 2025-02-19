using System.Collections.ObjectModel;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using EasySave.Models;

namespace EasySave.Views
{
    /// <summary>
    /// Logique d'interaction pour Home.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        public ObservableCollection<ModelJob> BackupJobs { get; set; }
        private static ResourceManager ResourceManager => BackupManager.GetInstance().resourceManager;

        public HomePage()
        {
            InitializeComponent();
            DataContext = this;
            Refresh();
        }

        private void Refresh()
        {
            MainWindow.GetInstance().Refresh();
            Title_Home.Text = ResourceManager.GetString("Home_Title");
            Header_BackupName.Header = ResourceManager.GetString("Home_Header_BackupName");
            Header_SourceDirectory.Header = ResourceManager.GetString("Home_Header_SourceDirectory");
            Header_TargetDirectory.Header = ResourceManager.GetString("Home_Header_TargetDirectory");
            Header_Type.Header = ResourceManager.GetString("Home_Header_Type");
            Header_Modify.Header = ResourceManager.GetString("Home_Header_Modify");
            Header_State.Header = ResourceManager.GetString("Home_Header_State");
            Button_Create.Content = ResourceManager.GetString("Home_Button_Create");
            Button_Execute.Content = ResourceManager.GetString("Home_Button_Execute");
            Button_Delete.Content = ResourceManager.GetString("Home_Button_Delete");
            DisplayBackupJobs();
        }

        private void BackupJobsListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (BackupJobsListView.View is GridView gridView)
            {
                double totalWidth = BackupJobsListView.ActualWidth - SystemParameters.VerticalScrollBarWidth;
                gridView.Columns[0].Width = totalWidth * 0.2;  // 20% pour "Nom de sauvegarde"
                gridView.Columns[1].Width = totalWidth * 0.25; // 25% pour "Répertoire source"
                gridView.Columns[2].Width = totalWidth * 0.25; // 25% pour "Répertoire cible"
                gridView.Columns[3].Width = totalWidth * 0.1;  // 10% pour "Type"
                gridView.Columns[4].Width = totalWidth * 0.1;  // 10% pour "Modifier"
                gridView.Columns[5].Width = totalWidth * 0.1;  // 10% pour "Etat"
            }
        }

        private void Button_Create_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.DataContext is ModelJob selectedJob)
                {
                    NavigationService.Navigate(new EditPage(selectedJob));
                }
                else
                {
                    NavigationService.Navigate(new EditPage());
                }
            }
        }

        private void Button_Execute_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
                {
                    MessageBox.Show(ResourceManager.GetString("Home_Message_BusinessSoftware"), ResourceManager.GetString("MessageTitle_Attention"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    var selectedItems = BackupJobsListView.SelectedItems.Cast<ModelJob>().ToList();

                    if (selectedItems.Count == 0)
                    {
                        MessageBox.Show(ResourceManager.GetString("Home_Message_NoneSelected"), ResourceManager.GetString("MessageTitle_NoneSelected"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var jobsToExecute = new List<ModelJob>(selectedItems);
                    var result = MessageBox.Show(ResourceManager.GetString("Home_Message_Execution"), ResourceManager.GetString("MessageTitle_Confirmation"), MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        foreach (var job in jobsToExecute)
                        {
                            int index = BackupManager.GetInstance().JsonConfig.BackupJobs.IndexOf(job);
                            if (index >= 0)
                            {
                                BackupManager.GetInstance().ExecuteBackupJobAsync(index);
                            }
                        }

                        BackupJobsListView.Items.Refresh();
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show(ResourceManager.GetString("Home_Message_ErrorExecution"), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedItems = BackupJobsListView.SelectedItems.Cast<ModelJob>().ToList();

                if (selectedItems.Count == 0)
                {
                    MessageBox.Show(ResourceManager.GetString("Home_Message_NoneSelected"), ResourceManager.GetString("MessageTitle_NoneSelected"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var jobsToDelete = new List<ModelJob>(selectedItems);
                var result = MessageBox.Show(ResourceManager.GetString("Home_Message_Deletion"), ResourceManager.GetString("MessageTitle_Confirmation"), MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (var job in jobsToDelete)
                    {
                        int index = BackupManager.GetInstance().JsonConfig.BackupJobs.IndexOf(job);
                        BackupManager.GetInstance().DeleteBackupJobAsync(index);
                    }

                    DisplayBackupJobs();
                    BackupJobsListView.Items.Refresh();
                }
            }
            catch (Exception)
            {
                MessageBox.Show(ResourceManager.GetString("Home_Message_ErrorDeletion"), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayBackupJobs()
        {
            BackupJobs = new ObservableCollection<ModelJob>(BackupManager.GetInstance().JsonConfig.BackupJobs);
        }
    }
}
