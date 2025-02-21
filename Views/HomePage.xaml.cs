using System;
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
            Title_Home.Text = ResourceManager.GetString("Title_Home");
            Header_BackupName.Header = ResourceManager.GetString("Text_BackupName");
            Header_SourceDirectory.Header = ResourceManager.GetString("Text_SourceDirectory");
            Header_TargetDirectory.Header = ResourceManager.GetString("Text_TargetDirectory");
            Header_Type.Header = ResourceManager.GetString("Text_Type");
            Header_Modify.Header = ResourceManager.GetString("Text_Modify");
            Header_State.Header = ResourceManager.GetString("Text_State");
            Button_Create.Content = ResourceManager.GetString("Button_Create");
            Button_Execute.Content = ResourceManager.GetString("Button_Execute");
            Button_Delete.Content = ResourceManager.GetString("Button_Delete");
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

        private async void Button_Execute_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
                {
                    MessageBox.Show(ResourceManager.GetString("Message_BusinessSoftware"), ResourceManager.GetString("MessageTitle_Attention"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    var selectedItems = BackupJobsListView.SelectedItems.Cast<ModelJob>().ToList();

                    if (selectedItems.Count == 0)
                    {
                        MessageBox.Show(ResourceManager.GetString("Message_Selection"), ResourceManager.GetString("MessageTitle_Attention"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var jobsToExecute = new List<ModelJob>(selectedItems);
                    var result = MessageBox.Show(ResourceManager.GetString("Message_Execute"), ResourceManager.GetString("MessageTitle_Confirmation"), MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        foreach (var job in jobsToExecute)
                        {
                            try
                            {
                                await BackupManager.GetInstance().ExecuteBackupJobAsync(job);
                                MessageBox.Show(string.Format(ResourceManager.GetString("Message_ExecuteSuccess"), job.Name), ResourceManager.GetString("MessageTitle_Success"), MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                if (ex.Message.Contains("Message_DirectoryNotFound"))
                                {
                                    MessageBox.Show(string.Format(ResourceManager.GetString("Message_DirectoryNotFound"), job.Name), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                else
                                {
                                    MessageBox.Show(string.Format(ResourceManager.GetString("Error"), ex.Message), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                        BackupJobsListView.Items.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(ResourceManager.GetString("Error"), ex.Message), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedItems = BackupJobsListView.SelectedItems.Cast<ModelJob>().ToList();

                if (selectedItems.Count == 0)
                {
                    MessageBox.Show(ResourceManager.GetString("Message_Selection"), ResourceManager.GetString("MessageTitle_Selection"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var jobsToDelete = new List<ModelJob>(selectedItems);
                var result = MessageBox.Show(ResourceManager.GetString("Message_Delete"), ResourceManager.GetString("MessageTitle_Confirmation"), MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (var job in jobsToDelete)
                    {
                        try
                        {
                            BackupJobs.Remove(job);
                            await BackupManager.GetInstance().DeleteBackupJobAsync(job);
                            MessageBox.Show(string.Format(ResourceManager.GetString("Message_DeleteSuccess"), job.Name), ResourceManager.GetString("MessageTitle_Success"), MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(string.Format(ResourceManager.GetString("Error"), ex.Message), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    Refresh();
                    BackupJobsListView.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(ResourceManager.GetString("Error"), ex.Message), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayBackupJobs()
        {
            BackupJobs = new ObservableCollection<ModelJob>(BackupManager.GetInstance().JsonConfig.BackupJobs);
        }
    }
}
