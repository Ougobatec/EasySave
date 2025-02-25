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
            DisplayBackupJobs();
        }

        /// <summary>
        /// BackupJobsListView size changed event
        /// </summary>
        private void BackupJobsListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (BackupJobsListView.View is GridView gridView)
            {
                double totalWidth = BackupJobsListView.ActualWidth - SystemParameters.VerticalScrollBarWidth;
                gridView.Columns[0].Width = totalWidth * 0.2;  // 20% for "Backup name"
                gridView.Columns[1].Width = totalWidth * 0.25; // 25% for "Source directory"
                gridView.Columns[2].Width = totalWidth * 0.25; // 25% for "Target directory"
                gridView.Columns[3].Width = totalWidth * 0.1;  // 10% for "Type"
                gridView.Columns[4].Width = totalWidth * 0.1;  // 10% for "Modify"
                gridView.Columns[5].Width = totalWidth * 0.1;  // 10% for "State"
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
                if (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
                {
                    MessageBox.Show(ResourceManager.GetString("Message_BusinessSoftware") ?? "Business software is running.", ResourceManager.GetString("MessageTitle_Attention") ?? "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    var selectedItems = BackupJobsListView.SelectedItems.Cast<ModelJob>().ToList();

                    if (selectedItems.Count == 0)
                    {
                        MessageBox.Show(ResourceManager.GetString("Message_Selection") ?? "Please select a job.", ResourceManager.GetString("MessageTitle_Attention") ?? "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var jobsToExecute = new List<ModelJob>(selectedItems);
                    var result = MessageBox.Show(ResourceManager.GetString("Message_Execute") ?? "Do you want to execute the selected jobs?", ResourceManager.GetString("MessageTitle_Confirmation") ?? "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        var backupTasks = jobsToExecute.Select(async job =>
                        {
                            try
                            {
                                await BackupManager.ExecuteBackupJobAsync(job);
                                MessageBox.Show(string.Format(ResourceManager.GetString("Message_ExecuteSuccess") ?? "Job {0} executed successfully.", job.Name), ResourceManager.GetString("MessageTitle_Success") ?? "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                if (ex.Message.Contains("Message_DirectoryNotFound"))
                                {
                                    MessageBox.Show(string.Format(ResourceManager.GetString("Message_DirectoryNotFound") ?? "Directory not found for job {0}.", job.Name), ResourceManager.GetString("MessageTitle_Error") ?? "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                else
                                {
                                    MessageBox.Show(string.Format(ResourceManager.GetString("Error") ?? "An error occurred: {0}", ex.Message), ResourceManager.GetString("MessageTitle_Error") ?? "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        });

                        await Task.WhenAll(backupTasks);
                        BackupJobsListView.Items.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(ResourceManager.GetString("Error") ?? "An error occurred: {0}", ex.Message), ResourceManager.GetString("MessageTitle_Error") ?? "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show(ResourceManager.GetString("Message_Selection") ?? "Please select a job.", ResourceManager.GetString("MessageTitle_Selection") ?? "Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var jobsToDelete = new List<ModelJob>(selectedItems);
                var result = MessageBox.Show(ResourceManager.GetString("Message_Delete") ?? "Do you want to delete the selected jobs?", ResourceManager.GetString("MessageTitle_Confirmation") ?? "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (var job in jobsToDelete)
                    {
                        try
                        {
                            BackupJobs.Remove(job);
                            await BackupManager.DeleteBackupJobAsync(job);
                            MessageBox.Show(string.Format(ResourceManager.GetString("Message_DeleteSuccess") ?? "Job {0} deleted successfully.", job.Name), ResourceManager.GetString("MessageTitle_Success") ?? "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(string.Format(ResourceManager.GetString("Error") ?? "An error occurred: {0}", ex.Message), ResourceManager.GetString("MessageTitle_Error") ?? "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    Refresh();
                    BackupJobsListView.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(ResourceManager.GetString("Error") ?? "An error occurred: {0}", ex.Message), ResourceManager.GetString("MessageTitle_Error") ?? "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
