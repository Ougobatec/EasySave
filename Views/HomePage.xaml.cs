﻿using System.Collections.ObjectModel;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using EasySave.Models;

namespace EasySave.Views
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private static BackupManager BackupManager => BackupManager.GetInstance();          // Backup manager instance
        private static ResourceManager ResourceManager => BackupManager.resourceManager;    // Resource manager instance
        public ObservableCollection<ModelJob> BackupJobs { get; set; } = [];                // List to get all backup jobs

        /// <summary>
        /// HomePage constructor to initialize the page and display backup jobs
        /// </summary>
        public HomePage()
        {
            InitializeComponent();
            DataContext = this;
            Refresh();
            DisplayBackupJobs();
        }

        /// <summary>
        /// Refresh the HomePage content
        /// </summary>
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
        }

        /// <summary>
        /// BackupJobsListView size changed event
        /// </summary>
        private void BackupJobsListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (BackupJobsListView.View is GridView gridView)
            {
                double totalWidth = BackupJobsListView.ActualWidth - SystemParameters.VerticalScrollBarWidth;
                gridView.Columns[0].Width = totalWidth * 0.10;  // 20% for "Backup name"
                gridView.Columns[1].Width = totalWidth * 0.23;  // 25% for "Source directory"
                gridView.Columns[2].Width = totalWidth * 0.23;  // 25% for "Target directory"
                gridView.Columns[3].Width = totalWidth * 0.08;  // 10% for "Type"
                gridView.Columns[4].Width = totalWidth * 0.08;  // 10% for "Modify"
                gridView.Columns[5].Width = totalWidth * 0.12;  // 10% for "State"
                gridView.Columns[6].Width = totalWidth * 0.16;  // 10% for "Actions"
            }
        }

        /// <summary>
        /// Button Create click event
        /// </summary>
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

        /// <summary>
        /// Button Execute click event
        /// </summary>
        private async void Button_Execute_Click(object sender, RoutedEventArgs e)
        {
            try
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
                    var backupTasks = jobsToExecute.Select(async job =>
                    {
                        await Execute(job);
                    });

                    await Task.WhenAll(backupTasks);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(ResourceManager.GetString("Error"), ex.Message), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Button Delete click event
        /// </summary>
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
                            await BackupManager.DeleteBackupJobAsync(job);
                            MessageBox.Show(string.Format(ResourceManager.GetString("Message_DeleteSuccess"), job.Name), ResourceManager.GetString("MessageTitle_Success"), MessageBoxButton.OK, MessageBoxImage.Information);
                            BackupJobs.Remove(job);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("Message_JobNotFound"))
                            {
                                MessageBox.Show(string.Format(ResourceManager.GetString("Message_JobNotFound"), job.Name), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else if (ex.Message.Contains("Message_Running"))
                            {
                                MessageBox.Show(string.Format(ResourceManager.GetString("Message_Running"), job.Name), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                MessageBox.Show(string.Format(ResourceManager.GetString("Error"), ex.Message), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(ResourceManager.GetString("Error"), ex.Message), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Button Play click event
        /// </summary>
        private async void Button_Play_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ModelJob job)
            {
                try
                {
                    await Execute(job);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(ResourceManager.GetString("Error"), ex.Message), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Button Pause click event
        /// </summary>
        private async void Button_Pause_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ModelJob job)
            {
                try
                {
                    await BackupManager.PauseBackupJobAsync(job);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Message_JobNotFound"))
                    {
                        MessageBox.Show(string.Format(ResourceManager.GetString("Message_JobNotFound"), job.Name), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else if (ex.Message.Contains("Message_NotRunning"))
                    {
                        MessageBox.Show(string.Format(ResourceManager.GetString("Message_NotRunning"), job.Name), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show(string.Format(ResourceManager.GetString("Error"), ex.Message), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Button Stop click event
        /// </summary>
        private async void Button_Stop_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ModelJob job)
            {
                try
                {
                    await BackupManager.StopBackupJobAsync(job);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Message_JobNotFound"))
                    {
                        MessageBox.Show(string.Format(ResourceManager.GetString("Message_JobNotFound"), job.Name), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else if (ex.Message.Contains("Message_AlreadyStopped"))
                    {
                        MessageBox.Show(string.Format(ResourceManager.GetString("Message_AlreadyStopped"), job.Name), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show(string.Format(ResourceManager.GetString("Error"), ex.Message), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Execute a backup job
        /// <param name="job">the backup job to execute</param>
        /// </summary>
        private async Task Execute(ModelJob job)
        {
            try
            {
                await BackupManager.ExecuteBackupJobAsync(job);
                MessageBox.Show(string.Format(ResourceManager.GetString("Message_ExecuteSuccess"), job.Name), ResourceManager.GetString("MessageTitle_Success"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Message_JobNotFound"))
                {
                    MessageBox.Show(string.Format(ResourceManager.GetString("Message_JobNotFound"), job.Name), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (ex.Message.Contains("Message_Running"))
                {
                    MessageBox.Show(string.Format(ResourceManager.GetString("Message_Running"), job.Name), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (ex.Message.Contains("Message_DirectoryNotFound"))
                {
                    MessageBox.Show(string.Format(ResourceManager.GetString("Message_DirectoryNotFound"), job.Name), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (ex.Message.Contains("Message_NotEnoughSpace"))
                {
                    MessageBox.Show(string.Format(ResourceManager.GetString("Message_NotEnoughSpace"), job.Name), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (ex.Message.Contains("Message_Paused"))
                {
                    MessageBox.Show(string.Format(ResourceManager.GetString("Message_Paused"), job.Name), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(string.Format(ResourceManager.GetString("Error"), ex.Message), ResourceManager.GetString("MessageTitle_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Display all backup jobs
        /// </summary>
        private void DisplayBackupJobs()
        {
            BackupJobs = [.. BackupManager.JsonConfig.BackupJobs];
        }

    }
}
