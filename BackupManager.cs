﻿using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
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
        private static readonly FileManager fileManager = FileManager.GetInstance();                                    // File manager instance
        private static readonly SemaphoreSlim semaphoreSlim = new(1);                                                   // Semaphore slim instance
        private static readonly ConcurrentDictionary<string, Task> runningBackups = new();                              // Running backups dictionary
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> fileLocks = new();                          // File locks dictionary
        private static readonly object configLock = new();                                                              // Config lock object
        private static readonly object stateLock = new();                                                               // State lock object
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
            SetCulture(JsonConfig.Language);                                                // Set culture
            Logger<ModelLog>.GetInstance().Settings(JsonConfig.LogFormat, LogDirectory);    // Set logger settings
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

            Task saveConfigTask;                                                                                // Save the config file task
            Task saveStateTask;                                                                                 // Save the state file task

            lock (configLock)
            {
                saveConfigTask = JsonManager.SaveJsonAsync(JsonConfig, ConfigFilePath);                         // Save the config file
            }
            lock (stateLock)
            {
                saveStateTask = JsonManager.SaveJsonAsync(JsonState, StateFilePath);                            // Save the state file
            }

            await Task.WhenAll(saveConfigTask, saveStateTask);                                                  // Wait for the tasks to complete
        }

        /// <summary>
        /// Update an existing backup job in the config and state files
        /// <param name="newJob">The new backup job</param>
        /// <param name="existingJob">The existing backup job to update</param>
        /// </summary>
        public async Task UpdateBackupJobAsync(ModelJob newJob, ModelJob job)
        {
            ModelJob? modelJob = JsonConfig.BackupJobs.FirstOrDefault(j => j.Name == job.Name);                                             // Get the backup job by name
            ModelState? modelState = JsonState.FirstOrDefault(s => s.Name == job.Name);                                                     // Get the job state by name

            if (modelJob == null)
            {
                throw new Exception("Message_JobNotFound");                                                                                 // Throw an exception if the job is not found
            }
            if (JsonConfig.BackupJobs.Any(j => j.Name.Equals(newJob.Name, StringComparison.OrdinalIgnoreCase) && j.Name != job.Name))
            {
                throw new Exception("Message_NameExists");                                                                                  // Throw an exception if the job name already exists
            }

            modelJob.Name = newJob.Name;                                                                                                    // Update the existing backup job name
            modelJob.SourceDirectory = newJob.SourceDirectory;                                                                              // Update the existing backup job source directory
            modelJob.TargetDirectory = newJob.TargetDirectory;                                                                              // Update the existing backup job target directory
            modelJob.Type = newJob.Type;                                                                                                    // Update the existing backup job type
            modelJob.State.Name = newJob.Name;                                                                                              // Update the existing backup job state name

            if (modelState == null)
            {
                JsonState.Add(modelJob.State);                                                                                              // Add the existing backup job state if not found
            }
            else
            {
                modelState.Name = newJob.Name;                                                                                              // Update the existing backup job state name
            }

            Task saveConfigTask;                                                                                                            // Save the config file task
            Task saveStateTask;                                                                                                             // Save the state file task

            lock (configLock)
            {
                saveConfigTask = JsonManager.SaveJsonAsync(JsonConfig, ConfigFilePath);                                                     // Save the config file
            }
            lock (stateLock)
            {
                saveStateTask = JsonManager.SaveJsonAsync(JsonState, StateFilePath);                                                        // Save the state file
            }

            await Task.WhenAll(saveConfigTask, saveStateTask);                                                                              // Wait for the tasks to complete
        }

        /// <summary>
        /// Execute an existing backup job and update its state
        /// <param name="job">The backup job to execute</param>
        /// </summary>
        public async Task ExecuteBackupJobAsync(ModelJob job)
        {
            if (runningBackups.ContainsKey(job.Name))
            {
                throw new Exception("Message_AlreadyRunning");                                                                                              // Throw an exception if the job is already running
            }

            ModelJob? modelJob = JsonConfig.BackupJobs.FirstOrDefault(j => j.Name == job.Name);                                                             // Get the backup job by name

            if (modelJob == null)
            {
                throw new Exception("Message_JobNotFound");                                                                                                 // Throw an exception if the job is not found
            }
            if (modelJob.State.State == "ACTIVE")
            {
                throw new Exception("Message_AlreadyActive");                                                                                               // Throw an exception if the job is already active
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

            var backupTask = CopyDirectoryAsync(modelJob);                                                                                                  // Start the backup process

            runningBackups[job.Name] = backupTask;                                                                                                          // Add the backup task to the running backups

            try
            {
                await backupTask;                                                                                                                           // Await the backup task
            }
            finally
            {
                runningBackups.TryRemove(job.Name, out _);                                                                                                  // Remove the backup task from the running backups
            }
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
            JsonConfig.BackupJobs.Remove(modelJob);                                             // Remove the backup job

            if (modelState != null)
            {
                JsonState.Remove(modelState);                                                   // Remove the job state
            }

            Task saveConfigTask;                                                                // Save the config file task
            Task saveStateTask;                                                                 // Save the state file task

            lock (configLock)
            {
                saveConfigTask = JsonManager.SaveJsonAsync(JsonConfig, ConfigFilePath);         // Save the config file
            }
            lock (stateLock)
            {
                saveStateTask = JsonManager.SaveJsonAsync(JsonState, StateFilePath);            // Save the state file
            }

            await Task.WhenAll(saveConfigTask, saveStateTask);                                  // Wait for the tasks to complete
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

            Task saveConfigTask;                                                                    // Save the config file task
            lock (configLock)
            {
                saveConfigTask = JsonManager.SaveJsonAsync(JsonConfig, ConfigFilePath);             // Save the config file
            }
            await saveConfigTask;                                                                   // Await the saveConfigTask to ensure the method is truly async
        }


        /// <summary>
        /// Copy a directory and its files to a target directory
        /// <param name="job">The backup job to execute</param>
        /// </summary>
        private async Task CopyDirectoryAsync(ModelJob job)
        {
            string sourceDir = job.SourceDirectory;
            string destDir = job.TargetDirectory;
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string saveDestDir = Path.Combine(destDir, $"{timestamp}_{job.Name}_{job.Type}");

            Directory.CreateDirectory(saveDestDir);                                                                                                                 // Create the target directory

            var backupStartTime = DateTime.Now;
            var extensionsToEncrypt = new HashSet<string>(JsonConfig.EncryptedExtensions, StringComparer.OrdinalIgnoreCase);                                        // Get the extensions to encrypt
            var priorityExtensions = new HashSet<string>(JsonConfig.PriorityExtensions, StringComparer.OrdinalIgnoreCase);                                          // Get the extensions to prioritize
            long limitSizeFile = JsonConfig.LimitSizeFile * 1024 * 1024;                                                                                            // Get the limit size of a file
            TimeSpan totalEncryptionTime = TimeSpan.Zero;

            IEnumerable<string> filesToCopy;                                                                                                                        // List of files to copy
            if (job.Type == BackupTypes.Full)
            {
                filesToCopy = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);                                                                    // Get all files in the source directory for a full backup
            }
            else
            {
                var previousBackups = Directory.GetDirectories(destDir).OrderByDescending(d => d).ToList();                                                         // Get the previous backups
                filesToCopy = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories)                                                                     // Get all files in the source directory for an incremental backup
                    .Where(file =>
                    {
                        string relativePath = file[(sourceDir.Length + 1)..];
                        return previousBackups.All(backupDir =>                                                                                                     // Check if the file is not in the previous backups or if it has been modified
                        {
                            string backupFilePath = Path.Combine(backupDir, relativePath);
                            return !File.Exists(backupFilePath) || new FileInfo(file).LastWriteTime > File.GetLastWriteTime(backupFilePath);                        // Check if the file is not in the backup or if it has been modified
                        });
                    });
            }

            filesToCopy = filesToCopy.OrderBy(file => priorityExtensions.Contains(Path.GetExtension(file)) ? 0 : 1);                                                // Order the files to copy by priority extensions

            var totalFilesToCopy = filesToCopy.Count();
            var nbFilesLeftToDo = totalFilesToCopy;
            var totalFilesSize = filesToCopy.Sum(file => new FileInfo(file).Length);

            await UpdateStateAsync(job, sourceDir, saveDestDir, "ACTIVE", totalFilesToCopy, totalFilesSize, nbFilesLeftToDo);                                       // Update the job state

            List<Task<TimeSpan>> copyTasks = [];                                                                                                                    // List of copy tasks
            SemaphoreSlim semaphore = new(3);                                                                                                                       // Max 3 files to copy at a time
            SemaphoreSlim largeFileSemaphore = new(1);                                                                                                              // Max 1 large file to copy at a time

            foreach (string file in filesToCopy)                                                                                                                    // Copy each file
            {
                var fileInfo = new FileInfo(file);
                string relativePath = file[(sourceDir.Length + 1)..];
                string destPath = Path.Combine(saveDestDir, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);                                                                                        // Create the target directory

                await UpdateStateAsync(job, file, destPath, "ACTIVE", totalFilesToCopy, totalFilesSize, nbFilesLeftToDo);                                           // Update the job state

                if (fileInfo.Length > limitSizeFile)
                {
                    await largeFileSemaphore.WaitAsync();                                                                                                           // Wait for the semaphore
                    await Task.WhenAll(copyTasks);                                                                                                                  // Wait for the copy tasks
                    copyTasks.Clear();                                                                                                                              // Clear the copy tasks

                    try
                    {
                        totalEncryptionTime += await CopyFileAsync(file, destPath, job, fileInfo, extensionsToEncrypt);                                             // Copy the large file
                    }
                    finally
                    {
                        largeFileSemaphore.Release();                                                                                                               // Release the semaphore
                    }
                }
                else
                {
                    copyTasks.Add(Task.Run(async () =>                                                                                                              // Add the copy task
                    {
                        await semaphore.WaitAsync();                                                                                                                // Wait for the semaphore
                        try
                        {
                            return await CopyFileAsync(file, destPath, job, fileInfo, extensionsToEncrypt);                                                         // Copy the file
                        }
                        finally
                        {
                            semaphore.Release();                                                                                                                    // Release the semaphore
                        }
                    }));
                }
                nbFilesLeftToDo--;                                                                                                                                  // Decrease the number of files left to do
            }

            var results = await Task.WhenAll(copyTasks);                                                                                                            // Wait for the copy tasks
            totalEncryptionTime += new TimeSpan(results.Sum(r => r.Ticks));

            var backupEndTime = DateTime.Now;
            var totalBackupTime = backupEndTime - backupStartTime;

            await semaphoreSlim.WaitAsync();                                                                                                                        // Wait for the semaphore
            try
            {
                await Logger<ModelLog>.GetInstance().Log(new ModelLog(job.Name, sourceDir, saveDestDir, totalFilesSize, totalEncryptionTime, totalBackupTime));
            }
            finally
            {
                semaphoreSlim.Release();                                                                                                                            // Release the semaphore
            }

            await UpdateStateAsync(job, "", "", "END", 0, 0, 0);                                                                                                    // Update the job state
        }

        /// <summary>
        /// Copy a file to a target directory and encrypt it if it has an encrypted extension
        /// <param name="source">The source file path</param>
        /// <param name="destination">The target file path</param>
        /// <param name="job">The backup job</param>
        /// <param name="fileInfo">The file info</param>
        /// <param name="encryptedExtensions">The encrypted extensions</param>
        /// </summary>
        private static async Task<TimeSpan> CopyFileAsync(string source, string destination, ModelJob job, FileInfo fileInfo, HashSet<string> encryptedExtensions)
        {
            var transferStartTime = DateTime.Now;

            var fileLock = fileLocks.GetOrAdd(destination, _ => new SemaphoreSlim(1, 1));                                                                   // Get the file lock
            await fileLock.WaitAsync();                                                                                                                     // Wait for the file lock

            try
            {
                using var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);                                            // Open the source file stream
                using var destinationStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None);                               // Open the destination file stream
                await sourceStream.CopyToAsync(destinationStream);                                                                                          // Copy the file
            }
            catch (Exception)
            {
                await semaphoreSlim.WaitAsync();                                                                                                            // Wait for the semaphore
                try
                {
                    await Logger<ModelLog>.GetInstance().Log(new ModelLog(job.Name, source, destination, fileInfo.Length, TimeSpan.Zero, TimeSpan.Zero));
                }
                finally
                {
                    semaphoreSlim.Release();                                                                                                                // Release the semaphore
                }
                return TimeSpan.Zero;                                                                                                                       // Return zero if an exception occurs
            }
            finally
            {
                fileLock.Release();                                                                                                                         // Release the file lock
                fileLocks.TryRemove(destination, out _);                                                                                                    // Remove the file lock
            }

            var transferEndTime = DateTime.Now;
            var transferTime = transferEndTime - transferStartTime;
            TimeSpan encryptionTime = TimeSpan.Zero;

            if (encryptedExtensions.Contains(fileInfo.Extension))                                                                                           // Check if the file has an encrypted extension
            {
                fileManager.Settings(destination, job.Key);                                                                                                 // Set the file manager settings
                encryptionTime = TimeSpan.FromMilliseconds(fileManager.TransformFile());                                                                    // Encrypt the file
            }

            await semaphoreSlim.WaitAsync();                                                                                                                // Wait for the semaphore
            try
            {
                await Logger<ModelLog>.GetInstance().Log(new ModelLog(job.Name, source, destination, fileInfo.Length, encryptionTime, transferTime));
            }
            finally
            {
                semaphoreSlim.Release();                                                                                                                    // Release the semaphore
            }

            return encryptionTime;
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
        private async Task UpdateStateAsync(ModelJob job, string source, string target, string state, int totalFilesToCopy, long totalFilesSize, int nbFilesLeftToDo)
        {
            job.State.SourceFilePath = source;                                                                                                      // Update the job state source file path
            job.State.TargetFilePath = target;                                                                                                      // Update the job state target file path
            job.State.State = state;                                                                                                                // Update the job state state value
            job.State.TotalFilesToCopy = totalFilesToCopy;                                                                                          // Update the job state total files to copy
            job.State.TotalFilesSize = totalFilesSize;                                                                                              // Update the job state total files size
            job.State.NbFilesLeftToDo = nbFilesLeftToDo;                                                                                            // Update the job state number of files left to do
            job.State.Progression = totalFilesToCopy == 0 ? 0 : (int)(((double)(totalFilesToCopy - nbFilesLeftToDo) / totalFilesToCopy) * 100);     // Update the job state progression

            ModelState? modelState = JsonState.FirstOrDefault(s => s.Name == job.Name);                                                             // Get the job state by name
            if (modelState != null)
            {
                JsonState.Remove(modelState);                                                                                                       // Remove the job state
            }
            JsonState.Add(job.State);                                                                                                               // Add the job state

            Task saveStateTask;                                                                                                                     // Save the state file task
            Task saveConfigTask;                                                                                                                    // Save the config file task

            lock (stateLock)
            {
                saveStateTask = JsonManager.SaveJsonAsync(JsonState, StateFilePath);                                                                // Save the state file
            }
            lock (configLock)
            {
                saveConfigTask = JsonManager.SaveJsonAsync(JsonConfig, ConfigFilePath);                                                             // Save the config file
            }

            await Task.WhenAll(saveStateTask, saveConfigTask);                                                                                      // Wait for the tasks to complete
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
