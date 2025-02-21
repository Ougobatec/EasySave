using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Controls;
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
        public ModelConfig JsonConfig { get; private set; } = new ModelConfig();
        public List<ModelState> JsonState { get; private set; } = [];
        private static BackupManager? BackupManager_Instance;
        private static readonly string ConfigFilePath = "Config\\config.json";
        private static readonly string StateFilePath = "Config\\state.json";
        private static readonly string LogDirectory = Path.Join(Path.GetTempPath(), "easysave\\logs");

        private BackupManager()
        {
            LoadConfig();
            LoadStates();
            SetCulture(JsonConfig.Language);
            Logger<ModelLog>.GetInstance().Settings(JsonConfig.LogFormat, LogDirectory);
        }

        public static BackupManager GetInstance()
        {
            BackupManager_Instance ??= new BackupManager();
            return BackupManager_Instance;
        }

        public async Task AddBackupJobAsync(ModelJob job)
        {
            if (JsonConfig.BackupJobs.Any(b => b.Name.Equals(job.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("Message_NameExists");
            }

            job.Key = GenerateKey(64);
            JsonConfig.BackupJobs.Add(job);
            JsonState.Add(new ModelState { Name = job.Name });
            await SaveJsonAsync(JsonConfig, ConfigFilePath);
            await SaveJsonAsync(JsonState, StateFilePath);
        }

        public async Task UpdateBackupJobAsync(ModelJob newJob, ModelJob existingJob)
        {
            if (JsonConfig.BackupJobs.Any(b => b.Name.Equals(newJob.Name, StringComparison.OrdinalIgnoreCase) && b != existingJob))
            {
                throw new Exception("Message_NameExists");
            }

            var state = JsonState.FirstOrDefault(s => s.Name == existingJob.Name);
            if (state != null)
            {
                state.Name = newJob.Name;
            }

            existingJob.Name = newJob.Name;
            existingJob.SourceDirectory = newJob.SourceDirectory;
            existingJob.TargetDirectory = newJob.TargetDirectory;
            existingJob.Type = newJob.Type;

            await SaveJsonAsync(JsonConfig, ConfigFilePath);
            await SaveJsonAsync(JsonState, StateFilePath);
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
            await UpdateStateAsync(state, "ACTIVE", job.SourceDirectory);
            await CopyDirectoryAsync(job);
            await UpdateStateAsync(state, "END", job.SourceDirectory, 0, 100);
        }


        /// <summary>
        /// delete a backup via a backup object
        /// </summary>
        public async Task DeleteBackupJobAsync(ModelJob job)
        {
            JsonConfig.BackupJobs.Remove(job);

            var state = JsonState.FirstOrDefault(s => s.Name == job.Name);
            if (state != null)
            {
                JsonState.Remove(state);
            }

            await SaveJsonAsync(JsonConfig, ConfigFilePath);
            await SaveJsonAsync(JsonState, StateFilePath);
        }

        //public async Task ChangeSettingsAsync(string? language = null, string? logFormat = null)
        public async Task ChangeSettingsAsync(string? parameter = null, string? value = null)
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
                // changement de la taille limite d'un fichier (pour qu'il soit considérer comme volumineux)
                case "limitSizeFile":
                    JsonConfig.limitSizeFile = Int32.Parse(value);
                    break;
                // ajout, suppréssion d'une extension prioritaire
                case "PriorityFiles":
                    
                    break;
                default:
                    // code block
                    break;
            }
            await SaveJsonAsync(JsonConfig, ConfigFilePath);
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
            var extensionsToEncrypt = new List<string> { ".txt", ".docx", ".jpg" };

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
                    await SaveJsonAsync(JsonState, StateFilePath);
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
                        await SaveJsonAsync(JsonState, StateFilePath);
                    }
                }
            }

            var backupEndTime = DateTime.Now;
            var totalBackupTime = backupEndTime - backupStartTime;
            long totalSize = Directory.GetFiles(destDir, "*.*", SearchOption.AllDirectories).Sum(f => new FileInfo(f).Length);

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
        /// c'est quoi ca?
        /// </summary>
        private async Task UpdateStateAsync(ModelState state, string newState, string sourceDir, int? nbFilesLeftToDo = null, int? progression = null)
        {
            state.State = newState;
            state.TotalFilesToCopy = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories).Length;
            state.TotalFilesSize = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories).Sum(f => new FileInfo(f).Length);
            state.NbFilesLeftToDo = nbFilesLeftToDo ?? state.TotalFilesToCopy;
            state.Progression = progression ?? (int)(((double)(state.TotalFilesToCopy - state.NbFilesLeftToDo) / state.TotalFilesToCopy) * 100);
            await SaveJsonAsync(JsonState, StateFilePath);
        }

        private void LoadConfig()
        {
            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    var json = File.ReadAllText(ConfigFilePath);                                            //lire tout le json du fichier
                    JsonConfig = JsonSerializer.Deserialize<ModelConfig>(json) ?? new ModelConfig();        //transform json to config data via ModelConfig class
                }
                catch (JsonException)
                {
                    JsonConfig = new ModelConfig();                                                         //je sais pas ce que c'est
                }
            }
            else
            {
                JsonConfig = new ModelConfig();                                                             //je sais pas ce que c'est
            }

            JsonConfig.Language ??= "en";
            JsonConfig.LogFormat ??= "json";
            JsonConfig.BackupJobs ??= [];
        }

        /// <summary>
        /// je sais pas ce que c'est
        /// </summary>
        private void LoadStates()
        {
            if (File.Exists(StateFilePath))
            {
                try
                {
                    var json = File.ReadAllText(StateFilePath);
                    JsonState = JsonSerializer.Deserialize<List<ModelState>>(json) ?? [];
                }
                catch (JsonException)
                {
                    JsonState = [];
                }
            }
            else
            {
                JsonState = [];
            }
        }

        /// <summary>
        /// save data into desired json file
        /// 
        /// mais le <T> je sais pas ce que c'est
        /// 
        /// </summary>
        private static async Task SaveJsonAsync<T>(T data, string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);                                                    //take the folder above the file
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);                                                           //create directory if it doesn't exist
            }

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });      //transform to json
            await File.WriteAllTextAsync(filePath, json);                                                       //write to desired json file
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



        /// <summary>
        /// create a cryptographic key via of the desired length via a random number generator
        /// </summary>
        private static string GenerateKey(int bits)
        {
            byte[] key = new byte[bits / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return Convert.ToBase64String(key);         //transform to base64
        }
    }
}

