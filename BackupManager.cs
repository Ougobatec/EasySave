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
    /// manage all the backup logic
    /// </summary>
    public class BackupManager
    {
        public ResourceManager resourceManager = new("EasySave.Resources.Resources", Assembly.GetExecutingAssembly());
        public ModelConfig JsonConfig { get; set; } = new ModelConfig();
        public List<ModelState> JsonState { get; set; } = [];
        private static BackupManager? BackupManager_Instance;
        private static readonly string ConfigFilePath = "Config\\config.json";
        private static readonly string StateFilePath = "Config\\state.json";
        private static readonly string LogDirectory = Path.Join(Path.GetTempPath(), "easysave\\logs");

        /// <summary>
        /// singleton isntancer
        /// </summary>
        public static BackupManager GetInstance()
        {
            BackupManager_Instance ??= new BackupManager();
            return BackupManager_Instance;
        }

        /// <summary>
        /// principal method how retrieve the state, config and language
        /// </summary>
        public void Load()
        {
            JsonManager.LoadConfig(ConfigFilePath);
            JsonManager.LoadStates(StateFilePath);
            SetCulture(JsonConfig.Language);
            Logger<ModelLog>.GetInstance().Settings(JsonConfig.LogFormat, LogDirectory);
        }

        /// <summary>
        /// where a backup is created add it to state and config file
        /// </summary>
        public async Task AddBackupJobAsync(ModelJob job)
        {
            if (JsonConfig.BackupJobs.Any(b => b.Name.Equals(job.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("Message_NameExists");
            }

            JsonConfig.BackupJobs.Add(job);
            JsonState.Add(job.State);
            await JsonManager.SaveJsonAsync(JsonConfig, ConfigFilePath);
            await JsonManager.SaveJsonAsync(JsonState, StateFilePath);
        }

        /// <summary>
        /// where a backup is updated modify it in config and state
        /// </summary>
        public async Task UpdateBackupJobAsync(ModelJob newJob, ModelJob existingJob)
        {
            if (JsonConfig.BackupJobs.Any(b => b.Name.Equals(newJob.Name, StringComparison.OrdinalIgnoreCase) && b != existingJob))
            {
                throw new Exception("Message_NameExists");
            }
            
            var job = JsonConfig.BackupJobs.FirstOrDefault(j => j.Name == existingJob.Name);
            job.Name = newJob.Name;
            job.SourceDirectory = newJob.SourceDirectory;
            job.TargetDirectory = newJob.TargetDirectory;
            job.Type = newJob.Type;
            job.State.Name = newJob.Name;

            var state = JsonState.FirstOrDefault(s => s.Name == existingJob.Name);
            state.Name = newJob.Name;

            await JsonManager.SaveJsonAsync(JsonConfig, ConfigFilePath);
            await JsonManager.SaveJsonAsync(JsonState, StateFilePath);
        }

        /// <summary>
        /// Executes the backup job asynchronously.
        /// </summary>
        /// <param name="job">The backup job to execute.</param>
        /// <exception cref="Exception">Thrown when the source directory is not found.</exception>
        public async Task ExecuteBackupJobAsync(ModelJob job)
        {
            if (!Directory.Exists(job.SourceDirectory))
            {
                throw new Exception("Message_DirectoryNotFound");
            }

            var state = JsonState.FirstOrDefault(s => s.Name == job.Name);

            // Get the size of the save
            long size = GetSizeRepertory(job.TargetDirectory);

            // Get the free space still available on the disk
            DriveInfo drive = new DriveInfo(job.TargetDirectory);

            if (drive.AvailableFreeSpace < size)
            {
                throw new Exception("Message_NotEnoughSpace");
            }
            else
            {
                await UpdateStateAsync(state, "ACTIVE", job.SourceDirectory);
                await CopyDirectoryAsync(job);
                await UpdateStateAsync(state, "END", job.SourceDirectory, 0, 100);
            }
        }

        /// <summary>
        /// Calculate the size of a repertory
        /// </summary>
        public long GetSizeRepertory(string sourceDir)
        {
            long size = 0;
            if (!Directory.Exists(sourceDir))
            {
                // if the repertory doesn't exist we set the size to -1
                return size = -1;
            }
            else
            {
                // Get all files and do a sum their size together
                foreach (string file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
                {
                    FileInfo fileInfo = new FileInfo(file);
                    size += fileInfo.Length;
                }
                return size;
            }
        }

        /// <summary>
        /// delete a backup via a backup object
        /// </summary>
        public async Task DeleteBackupJobAsync(ModelJob job)
        {
            JsonConfig.BackupJobs.Remove(job);
            JsonState.Remove(JsonState.FirstOrDefault(s => s.Name == job.Name));

            await JsonManager.SaveJsonAsync(JsonConfig, ConfigFilePath);
            await JsonManager.SaveJsonAsync(JsonState, StateFilePath);
        }

        public async Task ChangeSettingsAsync(string parameter, string value, List<string>? list = null)
        {
            switch (parameter)
            {
                // changement de la langue
                case "language":
                    JsonConfig.Language = value;
                    SetCulture(value);
                    break;
                // changement du logFormat
                case "logFormat":
                    JsonConfig.LogFormat = value;
                    Logger<ModelLog>.GetInstance().Settings(JsonConfig.LogFormat, LogDirectory);
                    break;
                // changement de la taille limite d'un fichier (pour qu'il soit consid�rer comme volumineux)
                case "limitSizeFile":
                    JsonConfig.LimitSizeFile = Int32.Parse(value);
                    break;
                // ajout, suppression d'une extension prioritaire
                case "PriorityFiles":
                    if (list != null)
                    {
                        JsonConfig.PriorityExtensions = list;
                    }
                    break;
                case "EncryptedFiles":
                    if (list != null)
                    {
                        JsonConfig.EncryptedExtensions = list;
                    }
                    break;
                default:
                    // code block
                    break;
            }
            await JsonManager.SaveJsonAsync(JsonConfig, ConfigFilePath);
        }

        /// <summary>
        /// c'est sale et c'est gros
        /// </summary>
        private async Task CopyDirectoryAsync(ModelJob job)
        {
            var state = JsonState.FirstOrDefault(s => s.Name == job.Name);
            string sourceDir = job.SourceDirectory;
            string baseDestDir = job.TargetDirectory;
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string destDir = Path.Combine(baseDestDir, timestamp + "_" + job.Name + "_" + job.Type);
            Directory.CreateDirectory(destDir);

            var backupStartTime = DateTime.Now;
            var extensionsToEncrypt = new List<string>();

            foreach (string el in BackupManager.GetInstance().JsonConfig.EncryptedExtensions)
            {
                extensionsToEncrypt.Add(el);
            }

            if (job.Type == BackupTypes.Full)
            {
                foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(sourceDir, destDir));
                }

                foreach (string newPath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
                {
                    string destPath = newPath.Replace(sourceDir, destDir);
                    var fileInfo = new FileInfo(newPath);
                    var startTime = DateTime.Now;

                    await Task.Run(() => File.Copy(newPath, destPath, true));

                    if (extensionsToEncrypt.Contains(fileInfo.Extension))
                    {
                        var fileManager = new FileManager(destPath, job.Key);
                        int encryptionTime = fileManager.TransformFile();
                        var endTime = DateTime.Now;
                        var transferTime = endTime - startTime;

                        await Logger<ModelLog>.GetInstance().Log(new ModelLog
                        {
                            Timestamp = DateTime.Now,
                            BackupName = job.Name,
                            Source = newPath,
                            Destination = destPath,
                            Size = fileInfo.Length,
                            EncryptionTime = TimeSpan.FromMilliseconds(encryptionTime),
                            TransfertTime = transferTime
                        });
                    }
                    else
                    {
                        var endTime = DateTime.Now;
                        var transferTime = endTime - startTime;

                        await Logger<ModelLog>.GetInstance().Log(new ModelLog
                        {
                            Timestamp = DateTime.Now,
                            BackupName = job.Name,
                            Source = newPath,
                            Destination = destPath,
                            Size = fileInfo.Length,
                            TransfertTime = transferTime
                        });
                    }

                    state.SourceFilePath = newPath;
                    state.TargetFilePath = destPath;
                    state.NbFilesLeftToDo--;
                    state.Progression = (int)(((double)(state.TotalFilesToCopy - state.NbFilesLeftToDo) / state.TotalFilesToCopy) * 100);
                    await JsonManager.SaveJsonAsync(JsonState, StateFilePath);
                }
            }
            else if (job.Type == BackupTypes.Differential)
            {
                var backupDirs = Directory.GetDirectories(baseDestDir).OrderBy(d => d).ToList();

                foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(sourceDir, destDir));
                }

                foreach (string newPath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
                {
                    string relativePath = newPath[(sourceDir.Length + 1)..];
                    string destPath = Path.Combine(destDir, relativePath);
                    var fileInfo = new FileInfo(newPath);

                    bool fileModified = true;
                    foreach (var backupDir in backupDirs)
                    {
                        string backupFilePath = Path.Combine(backupDir, relativePath);
                        if (File.Exists(backupFilePath) && fileInfo.LastWriteTime <= File.GetLastWriteTime(backupFilePath))
                        {
                            fileModified = false;
                            break;
                        }
                    }

                    if (fileModified)
                    {
                        var startTime = DateTime.Now;

                        await Task.Run(() => File.Copy(newPath, destPath, true));

                        if (extensionsToEncrypt.Contains(fileInfo.Extension))
                        {
                            var fileManager = new FileManager(destPath, job.Key);
                            int encryptionTime = fileManager.TransformFile();
                            var endTime = DateTime.Now;
                            var transferTime = endTime - startTime;

                            await Logger<ModelLog>.GetInstance().Log(new ModelLog
                            {
                                Timestamp = DateTime.Now,
                                BackupName = job.Name,
                                Source = newPath,
                                Destination = destPath,
                                Size = fileInfo.Length,
                                EncryptionTime = TimeSpan.FromMilliseconds(encryptionTime),
                                TransfertTime = transferTime
                            });
                        }
                        else
                        {
                            var endTime = DateTime.Now;
                            var transferTime = endTime - startTime;

                            await Logger<ModelLog>.GetInstance().Log(new ModelLog
                            {
                                Timestamp = DateTime.Now,
                                BackupName = job.Name,
                                Source = newPath,
                                Destination = destPath,
                                Size = fileInfo.Length,
                                TransfertTime = transferTime
                            });
                        }


                        state.SourceFilePath = newPath;
                        state.TargetFilePath = destPath;
                        state.NbFilesLeftToDo--;
                        state.Progression = (int)(((double)(state.TotalFilesToCopy - state.NbFilesLeftToDo) / state.TotalFilesToCopy) * 100);
                        await JsonManager.SaveJsonAsync(JsonState, StateFilePath);
                    }
                }
            }

            var backupEndTime = DateTime.Now;
            var totalBackupTime = backupEndTime - backupStartTime;
            long totalSize = Directory.GetFiles(destDir, "*.*", SearchOption.AllDirectories).Sum(f => new FileInfo(f).Length);

            /// <summary>
            /// pour l'utilisation de la classe logger dans la dll
            /// </summary>
            await Logger<ModelLog>.GetInstance().Log(new ModelLog
            {
                Timestamp = DateTime.Now,
                BackupName = job.Name,
                Source = sourceDir,
                Destination = destDir,
                Size = totalSize,
                TransfertTime = totalBackupTime
            });
        }

        /// <summary>
        /// 
        /// update of a new state if it change
        /// </summary>
        private async Task UpdateStateAsync(ModelState state, string newState, string sourceDir, int? nbFilesLeftToDo = null, int? progression = null)
        {
            state.State = newState;
            state.TotalFilesToCopy = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories).Length;
            state.TotalFilesSize = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories).Sum(f => new FileInfo(f).Length);
            state.NbFilesLeftToDo = nbFilesLeftToDo ?? state.TotalFilesToCopy;
            state.Progression = progression ?? (int)(((double)(state.TotalFilesToCopy - state.NbFilesLeftToDo) / state.TotalFilesToCopy) * 100);
            await JsonManager.SaveJsonAsync(JsonState, StateFilePath);
        }

        /// <summary>
        /// for changing language via resx file
        /// </summary>
        private static void SetCulture(string cultureName)
        {
            CultureInfo culture = new(cultureName);                 // to add more languages
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}
