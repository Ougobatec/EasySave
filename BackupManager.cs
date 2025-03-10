﻿using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using CryptoSoft;
using EasySave.Enumerations;
using EasySave.Models;
using Logger;

namespace EasySave
{
    /// <summary>
    /// Backup manager class to manage the backup jobs
    /// </summary>
    public class BackupManager
    {
        public ResourceManager resourceManager = new("EasySave.Resources.Resources", Assembly.GetExecutingAssembly());  // Resource manager instance
        public ModelConfig JsonConfig { get; set; } = new ModelConfig();                                                // Config model instance
        public List<ModelState> JsonState { get; set; } = [];                                                           // State model instance
        private static BackupManager? BackupManager_Instance;                                                           // Backup manager instance
        private static readonly Semaphore LargeFileSemaphore = new(1, 1);                                               // Semaphore for large files
        private static readonly Semaphore SmallFileSemaphore = new(3, 3);                                               // Semaphore for small files
        private static readonly string ConfigFilePath = "Config\\config.json";                                          // Config file path
        private static readonly string StateFilePath = "Config\\state.json";                                            // State file path
        private static readonly string LogDirectory = Path.Join(Path.GetTempPath(), "easysave\\logs");                  // Log directory path

        /// <summary>
        /// Backup manager constructor to load the config and state files and set the culture and logger
        /// </summary>
        private BackupManager()
        {
            JsonConfig = JsonManager.LoadJson(ConfigFilePath, new ModelConfig());           // Load config file
            JsonState = JsonManager.LoadJson(StateFilePath, new List<ModelState>());        // Load state file
            Logger<ModelLog>.GetInstance().Settings(JsonConfig.LogFormat, LogDirectory);    // Set logger settings
            SetCulture(JsonConfig.Language);                                                // Set culture
        }

        /// <summary>
        /// Get the backup manager instance or create it if it doesn't exist
        /// </summary>
        public static BackupManager GetInstance()
        {
            BackupManager_Instance ??= new BackupManager();     // Create backup manager instance if not exists
            return BackupManager_Instance;                      // Return backup manager instance
        }

        /// <summary>
        /// Add a new backup job to the config and state files
        /// <param name="job">The backup job to add</param>
        /// </summary>
        public async Task AddBackupJobAsync(ModelJob job)
        {
            if (JsonConfig.BackupJobs.Any(b => b.Name.Equals(job.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("Message_NameExists");                                                      // Throw an exception if the job name already exists
            }

            JsonConfig.BackupJobs.Add(job);                                                                     // Add the backup job to the config file
            JsonState.Add(job.State);                                                                           // Add the backup job state to the state file

            await JsonManager.SaveJsonAsync(JsonConfig, ConfigFilePath);                                        // Save the config file
            await JsonManager.SaveJsonAsync(JsonState, StateFilePath);                                          // Save the state file
        }

        /// <summary>
        /// Update an existing backup job in the config and state files
        /// <param name="newJob">The new backup job</param>
        /// <param name="existingJob">The existing backup job to update</param>
        /// </summary>
        public async Task UpdateBackupJobAsync(ModelJob newJob, ModelJob job)
        {
            ModelJob? modelJob = JsonConfig.BackupJobs.FirstOrDefault(j => j.Name == job.Name);                                         // Get the backup job by name
            ModelState? modelState = JsonState.FirstOrDefault(s => s.Name == job.Name);                                                 // Get the job state by name

            if (modelJob == null)
            {
                throw new Exception("Message_JobNotFound");                                                                             // Throw an exception if the job is not found
            }
            if (modelJob.State.State == BackupStates.ACTIVE)
            {
                throw new Exception("Message_Running");                                                                                 // Throw an exception if the job is running
            }
            if (JsonConfig.BackupJobs.Any(j => j.Name.Equals(newJob.Name, StringComparison.OrdinalIgnoreCase) && j.Name != job.Name))
            {
                throw new Exception("Message_NameExists");                                                                              // Throw an exception if the job name already exists
            }

            modelJob.Name = newJob.Name;                                                                                                // Update the existing backup job name
            modelJob.SourceDirectory = newJob.SourceDirectory;                                                                          // Update the existing backup job source directory
            modelJob.TargetDirectory = newJob.TargetDirectory;                                                                          // Update the existing backup job target directory
            modelJob.Type = newJob.Type;                                                                                                // Update the existing backup job type
            modelJob.State.Name = newJob.Name;                                                                                          // Update the existing backup job state name

            if (modelState == null)
            {
                JsonState.Add(modelJob.State);                                                                                          // Add the existing backup job state if not found
            }
            else
            {
                modelState.Name = newJob.Name;                                                                                          // Update the existing backup job state name
            }

            await JsonManager.SaveJsonAsync(JsonConfig, ConfigFilePath);                                                                // Save the config file
            await JsonManager.SaveJsonAsync(JsonState, StateFilePath);                                                                  // Save the state file
        }

        /// <summary>
        /// Delete an existing backup job from the config and state files
        /// <param name="job">The backup job to delete</param>
        /// </summary>
        public async Task DeleteBackupJobAsync(ModelJob job)
        {
            ModelJob? modelJob = JsonConfig.BackupJobs.FirstOrDefault(j => j.Name == job.Name); // Get the backup job by name
            ModelState? modelState = JsonState.FirstOrDefault(s => s.Name == job.Name);         // Get the job state by name

            if (modelJob == null)
            {
                throw new Exception("Message_JobNotFound");                                     // Throw an exception if the job is not found
            }
            if (modelJob.State.State == BackupStates.ACTIVE)
            {
                throw new Exception("Message_Running");                                         // Throw an exception if the job is running
            }

            JsonConfig.BackupJobs.Remove(modelJob);                                             // Remove the backup job

            if (modelState != null)
            {
                JsonState.Remove(modelState);                                                   // Remove the job state
            }

            await JsonManager.SaveJsonAsync(JsonConfig, ConfigFilePath);                        // Save the config file
            await JsonManager.SaveJsonAsync(JsonState, StateFilePath);                          // Save the state file
        }

        /// <summary>
        /// Execute an existing backup job and update its state
        /// <param name="job">The backup job to execute</param>
        /// </summary>
        public async Task ExecuteBackupJobAsync(ModelJob job)
        {
            ModelJob? modelJob = JsonConfig.BackupJobs.FirstOrDefault(j => j.Name == job.Name);                                                             // Get the backup job by name

            if (modelJob == null)
            {
                throw new Exception("Message_JobNotFound");                                                                                                 // Throw an exception if the job is not found
            }
            if (modelJob.State.State == BackupStates.ACTIVE)
            {
                throw new Exception("Message_Running");                                                                                                     // Throw an exception if the job is running
            }
            if (!Directory.Exists(modelJob.SourceDirectory))
            {
                throw new Exception("Message_DirectoryNotFound");                                                                                           // Throw an exception if the source directory does not exist
            }

            long backupSize = Directory.EnumerateFiles(modelJob.SourceDirectory, "*", SearchOption.AllDirectories).Sum(file => new FileInfo(file).Length);  // Get the backup size
            DriveInfo drive = new(Path.GetPathRoot(modelJob.TargetDirectory) ?? string.Empty);                                                              // Get disk info from the target directory
            if (drive.AvailableFreeSpace < backupSize)
            {
                throw new Exception("Message_NotEnoughSpace");                                                                                              // Throw an exception if there is not enough space on the disk
            }

            await CopyDirectoryAsync(modelJob);                                                                                                             // Copy the directory and its files
        }

        /// <summary>
        /// Pause an existing backup job and update its state
        /// <param name="job">The backup job to stop</param>
        /// </summary>
        public async Task PauseBackupJobAsync(ModelJob job)
        {
            ModelJob? modelJob = JsonConfig.BackupJobs.FirstOrDefault(j => j.Name == job.Name);     // Get the backup job by name

            if (modelJob == null)
            {
                throw new Exception("Message_JobNotFound");                                         // Throw an exception if the job is not found
            }

            if (modelJob.State.State != BackupStates.ACTIVE)
            {
                throw new Exception("Message_NotRunning");                                          // Throw an exception if the job is not running
            }

            await UpdateStateAsync(modelJob, modelJob.SourceDirectory, modelJob.TargetDirectory, BackupStates.PAUSED, modelJob.State.LastSaveDirectory, modelJob.State.TotalFilesToCopy, modelJob.State.TotalFilesSize, modelJob.State.RemainingFiles);
        }

        /// <summary>
        /// Stop an existing backup job and update its state
        /// <param name="job">The backup job to stop</param>
        /// </summary>
        public async Task StopBackupJobAsync(ModelJob job)
        {
            ModelJob? modelJob = JsonConfig.BackupJobs.FirstOrDefault(j => j.Name == job.Name);             // Get the backup job by name

            if (modelJob == null)
            {
                throw new Exception("Message_JobNotFound");                                                 // Throw an exception if the job is not found
            }

            if (modelJob.State.State == BackupStates.READY)
            {
                throw new Exception("Message_AlreadyStopped");                                              // Throw an exception if the job is already stopped
            }

            await UpdateStateAsync(modelJob, modelJob.SourceDirectory, modelJob.TargetDirectory, BackupStates.PAUSED, modelJob.State.LastSaveDirectory, modelJob.State.TotalFilesToCopy, modelJob.State.TotalFilesSize, modelJob.State.RemainingFiles);

            if (Directory.Exists(modelJob.State.LastSaveDirectory))
            {
                Directory.Delete(modelJob.State.LastSaveDirectory, true);                                   // Delete the last save directory
            }
            await UpdateStateAsync(modelJob, "", "", BackupStates.READY, "", 0, 0, new List<string>());     // Update the job state
        }

        /// <summary>
        /// Change the settings of the application and save them in the config file
        /// <param name="parameter">The parameter to change</param>
        /// <param name="value">The new value of the parameter</param>
        /// <param name="list">The list of values to change</param>
        /// </summary>
        public async Task ChangeSettingsAsync(string parameter, string value, List<string>? list = null)
        {
            switch (parameter)
            {
                case "language":                                                                    // Change the language
                    JsonConfig.Language = value;
                    SetCulture(value);
                    break;
                case "logFormat":                                                                   // Change the log format
                    JsonConfig.LogFormat = value;
                    Logger<ModelLog>.GetInstance().Settings(JsonConfig.LogFormat, LogDirectory);
                    break;
                case "limitSizeFile":                                                               // Change the limit size of a file
                    JsonConfig.LimitSizeFile = Int32.Parse(value);
                    break;
                case "PriorityFiles":                                                               // Change the priority extensions
                    if (list != null)
                    {
                        JsonConfig.PriorityExtensions = list;
                    }
                    break;
                case "EncryptedFiles":                                                              // Change the encrypted extensions
                    if (list != null)
                    {
                        JsonConfig.EncryptedExtensions = list;
                    }
                    break;
                case "Add_BusinessSoftware":                                                        // Add a business software
                    if (value != null)
                    {
                        JsonConfig.BusinessSoftwares.Add(value);
                    }
                    break;
                case "Remove_BusinessSoftware":                                                     // Remove a business software
                    if (value != null)
                    {
                        JsonConfig.BusinessSoftwares.Remove(value);
                    }
                    break;
                default:                                                                            // Default case
                    break;
            }

            await JsonManager.SaveJsonAsync(JsonConfig, ConfigFilePath);                            // Save the config file
        }

        /// <summary>
        /// Copy a directory and its files to a target directory
        /// <param name="job">The backup job to execute</param>
        /// </summary>
        private async Task CopyDirectoryAsync(ModelJob job)
        {
            ModelJob? modelJob = JsonConfig.BackupJobs.FirstOrDefault(j => j.Name == job.Name);                                                         // Get the backup job by name

            if (modelJob == null)
            {
                throw new Exception("Message_JobNotFound");                                                                                             // Throw an exception if the job is not found
            }

            string sourceDir = modelJob.SourceDirectory;                                                                                                // Get the source directory
            string destDir = modelJob.TargetDirectory;                                                                                                  // Get the target directory

            var remainingFiles = modelJob.State.RemainingFiles;                                                                                         // Get the remaining files
            var totalFilesToCopy = modelJob.State.TotalFilesToCopy;                                                                                     // Get the total files to copy
            var totalFilesSize = modelJob.State.TotalFilesSize;                                                                                         // Get the total files size

            var extensionsToEncrypt = new HashSet<string>(JsonConfig.EncryptedExtensions, StringComparer.OrdinalIgnoreCase);                            // Get the encrypted extensions
            var priorityExtensions = new HashSet<string>(JsonConfig.PriorityExtensions, StringComparer.OrdinalIgnoreCase);                              // Get the priority extensions
            long limitSizeFile = JsonConfig.LimitSizeFile * 1024 * 1024;                                                                                // Get the limit size of a file

            string saveDestDir;

            IEnumerable<string> filesToCopy = Enumerable.Empty<string>();
            if (modelJob.State.RemainingFiles.Count == 0)
            {
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                saveDestDir = Path.Combine(destDir, $"{timestamp}_{modelJob.Name}_{modelJob.Type}");

                if (modelJob.Type == BackupTypes.Full)
                {
                    filesToCopy = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);                                                    // Get all files to copy for a full backup 
                }
                else
                {
                    if (Directory.Exists(destDir))
                    {
                        var previousBackups = Directory.GetDirectories(destDir).OrderByDescending(d => d).ToList();
                        filesToCopy = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories)                                                 // Get all files to copy for a differential backup
                            .Where(file =>
                            {
                                string relativePath = file[(sourceDir.Length + 1)..];
                                return previousBackups.All(backupDir =>
                                {
                                    string backupFilePath = Path.Combine(backupDir, relativePath);
                                    return !File.Exists(backupFilePath) || new FileInfo(file).LastWriteTime > File.GetLastWriteTime(backupFilePath);    // Check if the file is not in the previous backup or if it has been modified
                                });
                            });
                    }
                    else
                    {
                        filesToCopy = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);                                                // Get all files to copy for a full backup
                    }
                }

                filesToCopy = filesToCopy.OrderBy(file => priorityExtensions.Contains(Path.GetExtension(file)) ? 0 : 1);                                // Order the files to copy by priority extensions

                remainingFiles = filesToCopy.ToList();
                totalFilesToCopy = filesToCopy.Count();
                totalFilesSize = filesToCopy.Sum(file => new FileInfo(file).Length);
            }
            else
            {
                saveDestDir = modelJob.State.LastSaveDirectory;
            }

            Directory.CreateDirectory(saveDestDir);                                                                                                     // Create the save directory

            await UpdateStateAsync(modelJob, sourceDir, saveDestDir, BackupStates.ACTIVE, saveDestDir, totalFilesToCopy, totalFilesSize, remainingFiles.ToList());

            while (modelJob.State.RemainingFiles.Count > 0)                                                                                             // Copy the files one by one until there are no more files to copy
            {
                if (Process.GetProcesses().Any(p => JsonConfig.BusinessSoftwares.Contains(p.ProcessName)))                                              // Check if a business software is running
                {
                    await UpdateStateAsync(modelJob, modelJob.State.SourceFilePath, modelJob.State.TargetFilePath, BackupStates.PAUSED, modelJob.State.LastSaveDirectory, modelJob.State.TotalFilesToCopy, modelJob.State.TotalFilesSize, modelJob.State.RemainingFiles);
                    throw new Exception("Message_BusinessSoftwareRunning");
                }
                if (modelJob.State.State == BackupStates.PAUSED)
                {
                    break;                                                                                                                              // Break the loop if the job is paused
                }

                string file = modelJob.State.RemainingFiles[0];                                                                                         // Get the first file to copy
                var fileInfo = new FileInfo(file);                                                                                                      // Get the file info
                string relativePath = file[(sourceDir.Length + 1)..];                                                                                   // Get the relative path of the file
                string destPath = Path.Combine(saveDestDir, relativePath);                                                                              // Get the destination path
                Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);                                                                            // Create the directory if it does not exist

                await UpdateStateAsync(modelJob, file, destPath, modelJob.State.State, modelJob.State.LastSaveDirectory, modelJob.State.TotalFilesToCopy, modelJob.State.TotalFilesSize, modelJob.State.RemainingFiles);

                if (fileInfo.Length > limitSizeFile)                                                                                                    // Check if the file is a large file
                {
                    LargeFileSemaphore.WaitOne();                                                                                                       // Wait for the semaphore

                    try
                    {
                        await CopyFileAsync(modelJob, file, destPath, fileInfo, extensionsToEncrypt);                                                   // Copy the file
                    }
                    finally
                    {
                        LargeFileSemaphore.Release();                                                                                                   // Release the semaphore
                    }
                }
                else                                                                                                                                    // If the file is a small file
                {
                    SmallFileSemaphore.WaitOne();                                                                                                       // Wait for the semaphore

                    try
                    {
                        await CopyFileAsync(modelJob, file, destPath, fileInfo, extensionsToEncrypt);                                                   // Copy the file
                    }
                    finally
                    {
                        SmallFileSemaphore.Release();                                                                                                   // Release the semaphore
                    }
                }
                modelJob.State.RemainingFiles.RemoveAt(0);                                                                                              // Remove the file from the remaining files
            }

            if (modelJob.State.State != BackupStates.PAUSED)                                                                                            // Check if the job is not paused
            {
                await UpdateStateAsync(modelJob, "", "", BackupStates.READY, "", 0, 0, new List<string>());
            }
            else
            {
                throw new Exception("Message_Paused");                                                                                                  // Throw an exception if the job is paused
            }
        }

        /// <summary>
        /// Copy a file to a target directory and encrypt it if it has an encrypted extension
        /// <param name="source">The source file path</param>
        /// <param name="destination">The target file path</param>
        /// <param name="job">The backup job</param>
        /// <param name="fileInfo">The file info</param>
        /// <param name="encryptedExtensions">The encrypted extensions</param>
        /// </summary>
        private async Task<TimeSpan> CopyFileAsync(ModelJob job, string source, string destination, FileInfo fileInfo, HashSet<string> encryptedExtensions)
        {
            ModelJob? modelJob = JsonConfig.BackupJobs.FirstOrDefault(j => j.Name == job.Name);                                                             // Get the backup job by name

            if (modelJob == null)
            {
                throw new Exception("Message_JobNotFound");                                                                                                 // Throw an exception if the job is not found
            }

            var transferStartTime = DateTime.Now;

            try
            {
                using var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);                                            // Open the source file stream
                using var destinationStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None);                               // Open the destination file stream
                await sourceStream.CopyToAsync(destinationStream);                                                                                          // Copy the file
            }
            catch (Exception ex)
            {
                await UpdateStateAsync(modelJob, modelJob.State.SourceFilePath, modelJob.State.TargetFilePath, BackupStates.PAUSED, modelJob.State.LastSaveDirectory, modelJob.State.TotalFilesToCopy, modelJob.State.TotalFilesSize, modelJob.State.RemainingFiles);
                throw new Exception($"Message_CopyError: {ex.Message}");                                                                                    // Throw an exception if the file copy fails
            }

            var transferEndTime = DateTime.Now;
            var transferTime = transferEndTime - transferStartTime;

            TimeSpan encryptionTime = TimeSpan.Zero;

            if (encryptedExtensions.Contains(fileInfo.Extension))                                                                                           // Check if the file has an encrypted extension
            {
                FileManager.GetInstance().Settings(destination, job.Key);                                                                                   // Set the file manager settings
                encryptionTime = TimeSpan.FromMilliseconds(FileManager.GetInstance().TransformFile());                                                      // Encrypt the file
            }

            await Logger<ModelLog>.GetInstance().Log(new ModelLog(job.Name, source, destination, (fileInfo.Length/1024), encryptionTime, transferTime));           // Log the file transfer
            return encryptionTime;                                                                                                                          // Return the encryption time
        }

        /// <summary>
        /// Update the state of a backup job and save it to JsonState and JsonConfig
        /// <param name="job">The backup job to update</param>
        /// <param name="source">The source file path</param>
        /// <param name="target">The target file path</param>
        /// <param name="state">The state value</param>
        /// <param name="totalFilesToCopy">The total files to copy</param>
        /// <param name="totalFilesSize">The total files size</param>
        /// <param name="nbFilesLeftToDo">The number of files left to do</param>
        /// </summary>
        private async Task UpdateStateAsync(ModelJob job, string source, string target, BackupStates state, string lastSaveDir, int totalFilesToCopy, long totalFilesSize, List<string> remainingFiles)
        {
            ModelJob? modelJob = JsonConfig.BackupJobs.FirstOrDefault(j => j.Name == job.Name);                                                             // Get the backup job by name
            ModelState? modelState = JsonState.FirstOrDefault(s => s.Name == job.Name);                                                                     // Get the job state by name

            if (modelJob == null)
            {
                throw new Exception("Message_JobNotFound");                                                                                                 // Throw an exception if the job is not found
            }

            modelJob.State.SourceFilePath = source;                                                                                                         // Update the job state source file path
            modelJob.State.TargetFilePath = target;                                                                                                         // Update the job state target file path
            modelJob.State.State = state;                                                                                                                   // Update the job state state value
            modelJob.State.LastSaveDirectory = lastSaveDir;                                                                                                 // Update the job state last save directory
            modelJob.State.TotalFilesToCopy = totalFilesToCopy;                                                                                             // Update the job state total files to copy
            modelJob.State.TotalFilesSize = totalFilesSize;                                                                                                 // Update the job state total files size
            modelJob.State.RemainingFiles = remainingFiles;                                                                                                 // Update the job state remaining files
            modelJob.State.NbFilesLeftToDo = remainingFiles.Count();                                                                                        // Update the job state number of files left to do
            modelJob.State.Progression = totalFilesToCopy == 0 ? 0 : (int)((double)(totalFilesToCopy - remainingFiles.Count()) / totalFilesToCopy * 100);   // Update the job state progression

            if (modelState != null)
            {
                JsonState.Remove(modelState);                                                                                                               // Remove the job state
            }
            JsonState.Add(modelJob.State);                                                                                                                  // Add the job state

            await JsonManager.SaveJsonAsync(JsonConfig, ConfigFilePath);                                                                                    // Save the config file
            await JsonManager.SaveJsonAsync(JsonState, StateFilePath);                                                                                      // Save the state file
        }

        /// <summary>
        /// Set the culture of the application
        /// <param name="cultureName">The culture name</param>
        /// </summary>
        private static void SetCulture(string cultureName)
        {
            CultureInfo culture = new(cultureName);                 // Create a new culture info
            CultureInfo.DefaultThreadCurrentCulture = culture;      // Set the default thread current culture
            CultureInfo.DefaultThreadCurrentUICulture = culture;    // Set the default thread current UI culture
            Thread.CurrentThread.CurrentUICulture = culture;        // Set the current UI culture
        }
    }
}
