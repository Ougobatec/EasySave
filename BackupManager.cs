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
        /// Execute an existing backup job and update its state
        /// <param name="job">The backup job to execute</param>
        /// </summary>
        public async Task ExecuteBackupJobAsync(ModelJob job)
        {
            ModelJob? modelJob = JsonConfig.BackupJobs.FirstOrDefault(j => j.Name == job.Name);             // Get the backup job by name

            if (modelJob == null)
            {
                throw new Exception("Message_JobNotFound");                                                 // Throw an exception if the job is not found
            }
            if (modelJob.State.State == "ACTIVE")
            {
                throw new Exception("Message_AlreadyActive");                                               // Throw an exception if the job is already active
            }
            if (!Directory.Exists(modelJob.SourceDirectory))
            {
                throw new Exception("Message_DirectoryNotFound");                                           // Throw an exception if the source directory does not exist
            }

            long backupSize = Directory.EnumerateFiles(modelJob.SourceDirectory, "*", SearchOption.AllDirectories).Sum(file => new FileInfo(file).Length);
            DriveInfo drive = new DriveInfo(Path.GetPathRoot(modelJob.TargetDirectory) ?? string.Empty);    // Get disk info from the target directory
            if (drive.AvailableFreeSpace < backupSize)                                                      // Check if there is enough space on the disk
            {
                throw new Exception("Message_NotEnoughSpace");                                              // Throw an exception if there is not enough space on the disk
            }

            await CopyDirectoryAsync(modelJob);                                                             // Start the backup process
        }

        /// <summary>
        /// Delete an existing backup job from the config and state files
        /// <param name="job">The backup job to delete</param>
        /// </summary>
        public async Task DeleteBackupJobAsync(ModelJob job)
        {
            ModelJob? modelJob = JsonConfig.BackupJobs.FirstOrDefault(j => j.Name == job.Name);     // Get the backup job by name
            ModelState? modelState = JsonState.FirstOrDefault(s => s.Name == job.Name);             // Get the job state by name

            if (modelJob == null)
            {
                throw new Exception("Message_JobNotFound");                                         // Throw an exception if the job is not found
            }
            JsonConfig.BackupJobs.Remove(modelJob);                                                 // Remove the backup job

            if (modelState != null)
            {
                JsonState.Remove(modelState);                                                       // Remove the job state
            }

            await JsonManager.SaveJsonAsync(JsonConfig, ConfigFilePath);                            // Save the config file
            await JsonManager.SaveJsonAsync(JsonState, StateFilePath);                              // Save the state file
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
                default:                                                                            // Default case
                    break;
            }
            await JsonManager.SaveJsonAsync(JsonConfig, ConfigFilePath);                            // Save the config file
        }

        /// <summary>
        /// Get the total size of a directory
        /// <param name="sourceDir">The source directory</param>
        /// </summary>
        public long GetDirectorySize(string sourceDir)
        {
            if (!Directory.Exists(sourceDir))
            {
                return -1;                                                                                                          // Return -1 if the directory does not exist
            }

            return Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories).Sum(file => new FileInfo(file).Length);    // Return the total size of the directory
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

            Directory.CreateDirectory(saveDestDir);                                                                                             // Create the target directory

            var backupStartTime = DateTime.Now;
            var extensionsToEncrypt = new HashSet<string>(JsonConfig.EncryptedExtensions, StringComparer.OrdinalIgnoreCase);                    // Extensions to encrypt
            var priorityExtensions = new HashSet<string>(JsonConfig.PriorityExtensions, StringComparer.OrdinalIgnoreCase);                      // Priority extensions
            TimeSpan totalEncryptionTime = TimeSpan.Zero;

            IEnumerable<string> filesToCopy;                                                                                                    // Get the files to copy
            if (job.Type == BackupTypes.Full)
            {
                filesToCopy = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);                                                // Get all files to copy if the backup type is full
            }
            else
            {
                var previousBackups = Directory.GetDirectories(destDir).OrderByDescending(d => d).ToList();                                     // Get the previous backups
                filesToCopy = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories)                                                 // Get the files to copy if the backup type is differential
                    .Where(file =>
                    {
                        string relativePath = file.Substring(sourceDir.Length + 1);
                        return previousBackups.All(backupDir =>
                        {
                            string backupFilePath = Path.Combine(backupDir, relativePath);
                            return !File.Exists(backupFilePath) || new FileInfo(file).LastWriteTime > File.GetLastWriteTime(backupFilePath);    // Check if the file is more recent than the last backup
                        });
                    });
            }

            filesToCopy = filesToCopy.OrderBy(file => priorityExtensions.Contains(Path.GetExtension(file)) ? 0 : 1);                            // Prioritizing files with priority extensions

            var totalFilesToCopy = filesToCopy.Count();
            var nbFilesLeftToDo = filesToCopy.Count();
            var totalFilesSize = filesToCopy.Sum(file => new FileInfo(file).Length);

            await UpdateStateAsync(job, sourceDir, saveDestDir, "ACTIVE", totalFilesToCopy, totalFilesSize, nbFilesLeftToDo);                   // Update the job state

            foreach (string file in filesToCopy)
            {
                var fileInfo = new FileInfo(file);
                string relativePath = file.Substring(sourceDir.Length + 1);
                string destPath = Path.Combine(saveDestDir, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);                                                                    // Create the target directory

                await UpdateStateAsync(job, file, destPath, "ACTIVE", totalFilesToCopy, totalFilesSize, nbFilesLeftToDo);                       // Update the job state

                var transferStartTime = DateTime.Now;
                await Task.Run(() => File.Copy(file, destPath, true));                                                                          // Copy the file
                var transferEndTime = DateTime.Now;
                var transferTime = transferEndTime - transferStartTime;

                if (extensionsToEncrypt.Contains(fileInfo.Extension))                                                                           // Encrypt the file if it is in the encrypted extensions list
                {
                    var fileManager = new FileManager(destPath, job.Key);
                    TimeSpan encryptionTime = TimeSpan.FromMilliseconds(fileManager.TransformFile());
                    totalEncryptionTime += encryptionTime;
                    await Logger<ModelLog>.GetInstance().Log(new ModelLog(job.Name, file, destPath, fileInfo.Length, encryptionTime, transferTime));
                }
                else
                {
                    await Logger<ModelLog>.GetInstance().Log(new ModelLog(job.Name, file, destPath, fileInfo.Length, TimeSpan.Zero, transferTime));
                }

                nbFilesLeftToDo--;
            }

            var backupEndTime = DateTime.Now;
            var totalBackupTime = backupEndTime - backupStartTime;

            await Logger<ModelLog>.GetInstance().Log(new ModelLog(job.Name, sourceDir, saveDestDir, totalFilesSize, totalEncryptionTime, totalBackupTime));

            await UpdateStateAsync(job, "", "", "END", 0, 0, 0);                                                                                // Update the job state
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
            job.State.Progression = totalFilesToCopy == 0 ? 0 : (int)(((double)(totalFilesToCopy - nbFilesLeftToDo) / totalFilesToCopy) * 100);   // Update the job state progression

            ModelState? modelState = JsonState.FirstOrDefault(s => s.Name == job.Name);                                                             // Get the job state by name
            if (modelState != null)
            {
                JsonState.Remove(modelState);                                                                                                       // Remove the job state
            }
            JsonState.Add(job.State);                                                                                                               // Add the job state

            await JsonManager.SaveJsonAsync(JsonState, StateFilePath);                                                                              // Save the state file
            await JsonManager.SaveJsonAsync(JsonConfig, ConfigFilePath);                                                                            // Save the config file
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
