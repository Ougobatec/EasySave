using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text.Json;
using EasySave.Enumerations;
using EasySave.Models;
using Logger;

namespace EasySave
{
    public class BackupManager
    {
        public ModelConfig Config { get; private set; }
        public ResourceManager resourceManager;
        private List<ModelState> backupStates = [];
        private readonly List<ModelJob> backupJobs;
        private readonly Logger<ModelLog> logger;
        private static readonly string ConfigFilePath = Path.Join(Path.GetTempPath(), "easysave\\config.json");
        private static readonly string StateFilePath = Path.Join(Path.GetTempPath(), "easysave\\state.json");

        public BackupManager()
        {
            Directory.CreateDirectory(Path.Join(Path.GetTempPath(), "easysave"));
            LoadConfigAsync().Wait();
            Config ??= new ModelConfig { Language = "en", LogFormat = "json", BackupJobs = [] };
            backupJobs = Config.BackupJobs;
            SetCulture(Config.Language);
            LoadStatesAsync().Wait();
            logger = Logger<ModelLog>.GetInstance(Config.LogFormat);
        }

        public void SetCulture(string cultureName)
        {
            CultureInfo culture = new(cultureName);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            resourceManager = new ResourceManager("EasySave.Resources.Resources", Assembly.GetExecutingAssembly());
        }

        public async Task AddBackupJobAsync(ModelJob job)
        {
            if (backupJobs.Any(b => b.Name.Equals(job.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(resourceManager.GetString("Error_DuplicateBackupJob"));
            }

            backupJobs.Add(job);
            backupStates.Add(new ModelState { Name = job.Name });
            await SaveConfigAsync();
            await SaveStatesAsync();
        }

        public async Task UpdateBackupJobAsync(ModelJob updatedJob, int index)
        {
            var existingJob = backupJobs[index];

            if (backupJobs.Any(b => b.Name.Equals(updatedJob.Name, StringComparison.OrdinalIgnoreCase) && b != existingJob))
            {
                throw new InvalidOperationException(resourceManager.GetString("Error_DuplicateBackupJob"));
            }

            var state = backupStates.FirstOrDefault(s => s.Name == existingJob.Name);
            if (state != null)
            {
                state.Name = updatedJob.Name;
            }

            existingJob.Name = updatedJob.Name;
            existingJob.SourceDirectory = updatedJob.SourceDirectory;
            existingJob.TargetDirectory = updatedJob.TargetDirectory;
            existingJob.Type = updatedJob.Type;

            await SaveConfigAsync();
            await SaveStatesAsync();
        }

        public async Task ExecuteBackupJobAsync(int index)
        {
            var job = backupJobs[index];
            var state = backupStates.FirstOrDefault(s => s.Name == job.Name);

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
            var job = backupJobs[index];
            backupJobs.RemoveAt(index);

            var state = backupStates.FirstOrDefault(s => s.Name == job.Name);
            if (state != null)
            {
                backupStates.Remove(state);
            }

            await SaveConfigAsync();
            await SaveStatesAsync();
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

            await SaveConfigAsync();
        }

        private async Task UpdateStateAsync(ModelState state, string newState, string sourceDir, int? nbFilesLeftToDo = null, int? progression = null)
        {
            state.State = newState;
            state.TotalFilesToCopy = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories).Length;
            state.TotalFilesSize = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories).Sum(f => new FileInfo(f).Length);
            state.NbFilesLeftToDo = nbFilesLeftToDo ?? state.TotalFilesToCopy;
            state.Progression = progression ?? (int)(((double)(state.TotalFilesToCopy - state.NbFilesLeftToDo) / state.TotalFilesToCopy) * 100);
            await SaveStatesAsync();
        }

        private async Task CopyDirectoryAsync(ModelJob job)
        {
            var state = backupStates.FirstOrDefault(s => s.Name == job.Name);
            string sourceDir = job.SourceDirectory;
            string baseDestDir = job.TargetDirectory;
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string destDir = Path.Combine(baseDestDir, timestamp);
            Directory.CreateDirectory(destDir);

            var backupStartTime = DateTime.Now;

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

                    var endTime = DateTime.Now;
                    var transferTime = endTime - startTime;

                    await logger.Log(new ModelLog
                    {
                        Timestamp = DateTime.Now,
                        BackupName = job.Name,
                        Source = newPath,
                        Destination = destPath,
                        Size = fileInfo.Length,
                        TransfertTime = transferTime
                    }, Config.LogFormat);

                    state.SourceFilePath = newPath;
                    state.TargetFilePath = destPath;
                    state.NbFilesLeftToDo--;
                    state.Progression = (int)(((double)(state.TotalFilesToCopy - state.NbFilesLeftToDo) / state.TotalFilesToCopy) * 100);
                    await SaveStatesAsync();
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

                        var endTime = DateTime.Now;
                        var transferTime = endTime - startTime;

                        await logger.Log(new ModelLog
                        {
                            Timestamp = DateTime.Now,
                            BackupName = job.Name,
                            Source = newPath,
                            Destination = destPath,
                            Size = fileInfo.Length,
                            TransfertTime = transferTime
                        }, Config.LogFormat);

                        state.SourceFilePath = newPath;
                        state.TargetFilePath = destPath;
                        state.NbFilesLeftToDo--;
                        state.Progression = (int)(((double)(state.TotalFilesToCopy - state.NbFilesLeftToDo) / state.TotalFilesToCopy) * 100);
                        await SaveStatesAsync();
                    }
                }
            }

            var backupEndTime = DateTime.Now;
            var totalBackupTime = backupEndTime - backupStartTime;
            long totalSize = Directory.GetFiles(destDir, "*.*", SearchOption.AllDirectories).Sum(f => new FileInfo(f).Length);

            await logger.Log(new ModelLog
            {
                Timestamp = DateTime.Now,
                BackupName = job.Name,
                Source = sourceDir,
                Destination = destDir,
                Size = totalSize,
                TransfertTime = totalBackupTime
            }, Config.LogFormat);
        }

        private async Task LoadConfigAsync()
        {
            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(ConfigFilePath);
                    Config = JsonSerializer.Deserialize<ModelConfig>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true
                    }) ?? new ModelConfig { BackupJobs = [] };
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                    Config = new ModelConfig { BackupJobs = [] };
                }
            }
        }

        private async Task SaveConfigAsync()
        {
            var json = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(ConfigFilePath, json);
        }

        private async Task LoadStatesAsync()
        {
            if (File.Exists(StateFilePath))
            {
                var json = await File.ReadAllTextAsync(StateFilePath);
                backupStates = JsonSerializer.Deserialize<List<ModelState>>(json) ?? [];
            }
        }

        private async Task SaveStatesAsync()
        {
            var json = JsonSerializer.Serialize(backupStates, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(StateFilePath, json);
        }
    }
}
