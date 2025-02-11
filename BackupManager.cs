using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using EasySave.Models;
using Logger;

namespace EasySave
{
    public class BackupManager
    {
        public ModelConfig config { get; private set; }
        private const string ConfigFilePath = "..\\..\\..\\config.json";
        private const string StateFilePath = "..\\..\\..\\state.json";
        private readonly Logger<ModelLog> logger = Logger<ModelLog>.GetInstance();
        private List<ModelState> backupStates = new List<ModelState>();

        public BackupManager()
        {
            LoadConfigAsync().Wait();
            LoadStatesAsync().Wait();
        }

        public async Task AddBackupJobAsync(ModelJob job)
        {
            if (config.BackupJobs.Count >= 5)
            {
                throw new Exception("Cannot add more than 5 backup jobs.");
            }

            if (config.BackupJobs.Any(b => b.Name.Equals(job.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("A backup job with the same name already exists.");
            }

            config.BackupJobs.Add(job);
            backupStates.Add(new ModelState { Name = job.Name });
            await SaveConfigAsync();
            await SaveStatesAsync();
        }

        public async Task UpdateBackupJobAsync(int index, ModelJob updatedJob)
        {
            var existingJob = config.BackupJobs[index];

            if (config.BackupJobs.Any(b => b.Name.Equals(updatedJob.Name, StringComparison.OrdinalIgnoreCase) && b != existingJob))
            {
                throw new Exception("A backup job with the same name already exists.");
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
            var job = config.BackupJobs[index];
            var state = backupStates.FirstOrDefault(s => s.Name == job.Name);

            if (!Directory.Exists(job.SourceDirectory))
            {
                Console.WriteLine($"Source directory not found for '{job.Name}': {job.SourceDirectory}");
                return;
            }

            await UpdateStateAsync(state, "ACTIVE", job.SourceDirectory);
            await CopyDirectoryAsync(job.SourceDirectory, job.TargetDirectory, job.Name);
            await UpdateStateAsync(state, "END", job.SourceDirectory, 0, 100);

            Console.WriteLine($"Backup job '{job.Name}' executed.");
        }

        private async Task CopyDirectoryAsync(string sourceDir, string destDir, string backupName)
        {
            var state = backupStates.FirstOrDefault(s => s.Name == backupName);

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

                logger.Log(new ModelLog
                {
                    Timestamp = DateTime.Now,
                    BackupName = backupName,
                    Source = newPath,
                    Destination = destPath,
                    FileSize = fileInfo.Length,
                    TransfertTime = transferTime
                });

                state.SourceFilePath = newPath;
                state.TargetFilePath = destPath;
                state.NbFilesLeftToDo--;
                state.Progression = (int)(((double)(state.TotalFilesToCopy - state.NbFilesLeftToDo) / state.TotalFilesToCopy) * 100);
                await SaveStatesAsync();
            }
        }

        private async Task LoadConfigAsync()
        {
            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(ConfigFilePath);
                    config = JsonSerializer.Deserialize<ModelConfig>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true
                    }) ?? new ModelConfig { BackupJobs = new List<ModelJob>() };
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                    config = new ModelConfig { BackupJobs = new List<ModelJob>() };
                }
            }
        }

        private async Task SaveConfigAsync()
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(ConfigFilePath, json);
        }

        private async Task LoadStatesAsync()
        {
            if (File.Exists(StateFilePath))
            {
                var json = await File.ReadAllTextAsync(StateFilePath);
                backupStates = JsonSerializer.Deserialize<List<ModelState>>(json) ?? new List<ModelState>();
            }
        }

        private async Task SaveStatesAsync()
        {
            var json = JsonSerializer.Serialize(backupStates, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(StateFilePath, json);
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
    }
}