using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text.Json;
using System.Security.Cryptography;
using EasySave.Enumerations;
using EasySave.Models;
using Logger;
using CryptoSoft;

namespace EasySave
{
    public class BackupManager
    {
        public ResourceManager resourceManager = new("EasySave.Resources.Resources", Assembly.GetExecutingAssembly());
        public ModelConfig Config { get; private set; }
        private List<ModelState> BackupStates = [];
        private static BackupManager BackupManager_Instance = null;
        private static readonly string ConfigFilePath = "Config\\config.json";
        private static readonly string StateFilePath = "Config\\state.json";
        private static readonly string LogDirectory = Path.Join(Path.GetTempPath(), "easysave\\logs");

        public static BackupManager GetInstance()
        {
            BackupManager_Instance ??= new BackupManager();
            return BackupManager_Instance;
        }

        private BackupManager()
        {
            LoadConfigAsync();
            LoadStatesAsync();
            SetCulture(Config.Language);
            Logger<ModelLog>.GetInstance().Settings(Config.LogFormat, LogDirectory);
        }

        public static void SetCulture(string cultureName)
        {
            CultureInfo culture = new(cultureName);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }

        public async Task AddBackupJobAsync(ModelJob job)
        {
            if (Config.BackupJobs.Any(b => b.Name.Equals(job.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(resourceManager.GetString("Error_DuplicateBackupJob"));
            }

            job.Key = GenerateKey(64);
            Config.BackupJobs.Add(job);
            BackupStates.Add(new ModelState { Name = job.Name });
            await SaveJSONAsync(Config, ConfigFilePath);
            await SaveJSONAsync(BackupStates, StateFilePath);
        }

        public async Task UpdateBackupJobAsync(ModelJob updatedJob, int index)
        {
            var existingJob = Config.BackupJobs[index];

            if (Config.BackupJobs.Any(b => b.Name.Equals(updatedJob.Name, StringComparison.OrdinalIgnoreCase) && b != existingJob))
            {
                throw new InvalidOperationException(resourceManager.GetString("Error_DuplicateBackupJob"));
            }

            var state = BackupStates.FirstOrDefault(s => s.Name == existingJob.Name);
            if (state != null)
            {
                state.Name = updatedJob.Name;
            }

            existingJob.Name = updatedJob.Name;
            existingJob.SourceDirectory = updatedJob.SourceDirectory;
            existingJob.TargetDirectory = updatedJob.TargetDirectory;
            existingJob.Type = updatedJob.Type;

            await SaveJSONAsync(Config, ConfigFilePath);
            await SaveJSONAsync(BackupStates, StateFilePath);
        }

        public async Task ExecuteBackupJobAsync(int index)
        {
            var job = Config.BackupJobs[index];
            var state = BackupStates.FirstOrDefault(s => s.Name == job.Name);

            if (!Directory.Exists(job.SourceDirectory))
            {
                Console.WriteLine(string.Format(resourceManager.GetString("Error_SourceDirectoryNotFound"), job.Name, job.SourceDirectory));
                return;
            }

            await UpdateStateAsync(state, "ACTIVE", job.SourceDirectory);
            await CopyDirectoryAsync(job);
            await UpdateStateAsync(state, "END", job.SourceDirectory, 0, 100);

            Console.WriteLine(string.Format(resourceManager.GetString("Message_BackupJobExecuted"), job.Name));
        }

        public async Task DeleteBackupJobAsync(int index)
        {
            var job = Config.BackupJobs[index];
            Config.BackupJobs.RemoveAt(index);

            var state = BackupStates.FirstOrDefault(s => s.Name == job.Name);
            if (state != null)
            {
                BackupStates.Remove(state);
            }

            await SaveJSONAsync(Config, ConfigFilePath);
            await SaveJSONAsync(BackupStates, StateFilePath);
        }

        public async Task ChangeSettingsAsync(string language, string logFormat)
        {
            if (!string.IsNullOrEmpty(language))
            {
                Config.Language = language;
                SetCulture(language);
            }

            if (!string.IsNullOrEmpty(logFormat))
            {
                Config.LogFormat = logFormat;
            }

            await SaveJSONAsync(Config, ConfigFilePath);
            Logger<ModelLog>.GetInstance().Settings(Config.LogFormat, LogDirectory);
        }

        private async Task UpdateStateAsync(ModelState state, string newState, string sourceDir, int? nbFilesLeftToDo = null, int? progression = null)
        {
            state.State = newState;
            state.TotalFilesToCopy = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories).Length;
            state.TotalFilesSize = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories).Sum(f => new FileInfo(f).Length);
            state.NbFilesLeftToDo = nbFilesLeftToDo ?? state.TotalFilesToCopy;
            state.Progression = progression ?? (int)(((double)(state.TotalFilesToCopy - state.NbFilesLeftToDo) / state.TotalFilesToCopy) * 100);
            await SaveJSONAsync(BackupStates, StateFilePath);
        }

        private async Task CopyDirectoryAsync(ModelJob job)
        {
            var state = BackupStates.FirstOrDefault(s => s.Name == job.Name);
            string sourceDir = job.SourceDirectory;
            string baseDestDir = job.TargetDirectory;
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string destDir = Path.Combine(baseDestDir, timestamp);
            Directory.CreateDirectory(destDir);

            var backupStartTime = DateTime.Now;
            var extensionsToEncrypt = new List<string> { ".txt", ".docx", ".jpg" }; // Extensions des fichiers à crypter

            if (job.Type == BackupTypes.Full)
            {
                // Sauvegarde complète : copier tous les fichiers
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
                            EncryptionTime = encryptionTime,
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
                    await SaveJSONAsync(BackupStates, StateFilePath);
                }
            }
            else if (job.Type == BackupTypes.Differential)
            {
                // Trouver tous les sous-dossiers de sauvegarde
                var backupDirs = Directory.GetDirectories(baseDestDir).OrderBy(d => d).ToList();

                // Sauvegarde différentielle : copier uniquement les fichiers modifiés ou nouveaux
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
                                EncryptionTime = encryptionTime,
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
                        await SaveJSONAsync(BackupStates, StateFilePath);
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


        private void LoadConfigAsync()
        {
            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    var json = File.ReadAllText(ConfigFilePath);
                    Config = JsonSerializer.Deserialize<ModelConfig>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true
                    }) ?? new ModelConfig();
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                    Config = new ModelConfig();
                }
            }
            else
            {
                Config = new ModelConfig();
            }

            Config.Language ??= "en";
            Config.LogFormat ??= "json";
            Config.BackupJobs ??= [];
        }

        private void LoadStatesAsync()
        {
            if (File.Exists(StateFilePath))
            {
                var json =  File.ReadAllText(StateFilePath);
                BackupStates = JsonSerializer.Deserialize<List<ModelState>>(json) ?? [];
            }
        }

        private static async Task SaveJSONAsync<T>(T data, string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }

        private static string GenerateKey(int bits)
        {
            byte[] key = new byte[bits/8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return Convert.ToBase64String(key);
        }
    }
}

