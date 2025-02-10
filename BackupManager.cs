using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EasySave.Models;
using Logger;

namespace EasySave
{
    public class BackupManager
    {
        private List<ModelJob> backupJobs = new List<ModelJob>();
        public List<ModelJob> BackupJobs => backupJobs;

        private const string ConfigFilePath = "..\\..\\..\\config.json";
        private const string StateFilePath = "..\\..\\..\\state.json";
        private readonly Logger<ModelLog> logger = Logger<ModelLog>.GetInstance();
        private List<ModelState> backupStates = new List<ModelState>();

        public BackupManager()
        {
            LoadBackupJobsAsync().Wait();
            LoadBackupStatesAsync().Wait();
        }

        public async Task AddBackupJobAsync(ModelJob job)
        {
            if (backupJobs.Count >= 5)
            {
                throw new Exception("Cannot add more than 5 backup jobs.");
            }

            if (backupJobs.Any(b => b.Name.Equals(job.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("A backup job with the same name already exists.");
            }

            backupJobs.Add(job);
            backupStates.Add(new ModelState { Name = job.Name });
            await SaveBackupJobsAsync();
            await SaveBackupStatesAsync();
        }

        public async Task UpdateBackupJobAsync(int index, ModelJob updatedJob)
        {
            var existingJob = backupJobs[index];

            if (backupJobs.Any(b => b.Name.Equals(updatedJob.Name, StringComparison.OrdinalIgnoreCase) && b != existingJob))
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

            await SaveBackupJobsAsync();
            await SaveBackupStatesAsync();
        }

        public async Task ExecuteBackupJobAsync(int index)
        {
            var job = backupJobs[index];
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

        public async Task ExecuteAllBackupJobsAsync()
        {
            foreach (var job in backupJobs)
            {
                var state = backupStates.FirstOrDefault(s => s.Name == job.Name);

                if (!Directory.Exists(job.SourceDirectory))
                {
                    Console.WriteLine($"Source directory not found for '{job.Name}': {job.SourceDirectory}");
                    continue;
                }

                await UpdateStateAsync(state, "ACTIVE", job.SourceDirectory);

                await CopyDirectoryAsync(job.SourceDirectory, job.TargetDirectory, job.Name);

                await UpdateStateAsync(state, "END", job.SourceDirectory, 0, 100);

                Console.WriteLine($"Backup job '{job.Name}' executed.");
            }
        }

        private async Task CopyDirectoryAsync(string sourceDir, string destDir, string backupName)
        {
            var state = backupStates.FirstOrDefault(s => s.Name == backupName);

            // Create all directories
            foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourceDir, destDir));
            }

            // Copy all the files
            foreach (string newPath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
            {
                string destPath = newPath.Replace(sourceDir, destDir);
                var fileInfo = new FileInfo(newPath);
                var startTime = DateTime.Now;

                await Task.Run(() => File.Copy(newPath, destPath, true));

                var endTime = DateTime.Now;
                var transferTime = endTime - startTime;

                // Log the file copy operation
                logger.Log(new ModelLog
                {
                    Timestamp = DateTime.Now,
                    BackupName = backupName,
                    Source = newPath,
                    Destination = destPath,
                    FileSize = fileInfo.Length,
                    TransfertTime = transferTime
                });

                // Update state
                state.SourceFilePath = newPath;
                state.TargetFilePath = destPath;
                state.NbFilesLeftToDo--;
                state.Progression = (int)(((double)(state.TotalFilesToCopy - state.NbFilesLeftToDo) / state.TotalFilesToCopy) * 100);
                await SaveBackupStatesAsync();
            }
        }

        private async Task UpdateStateAsync(ModelState state, string newState, string sourceDir, int? nbFilesLeftToDo = null, int? progression = null)
        {
            state.State = newState;
            state.TotalFilesToCopy = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories).Length;
            state.TotalFilesSize = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories).Sum(f => new FileInfo(f).Length);
            state.NbFilesLeftToDo = nbFilesLeftToDo ?? state.TotalFilesToCopy;
            state.Progression = progression ?? (int)(((double)(state.TotalFilesToCopy - state.NbFilesLeftToDo) / state.TotalFilesToCopy) * 100);
            await SaveBackupStatesAsync();
        }

        private async Task LoadBackupJobsAsync()
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = await File.ReadAllTextAsync(ConfigFilePath);
                backupJobs = JsonSerializer.Deserialize<List<ModelJob>>(json) ?? new List<ModelJob>();
            }
        }

        private async Task SaveBackupJobsAsync()
        {
            var json = JsonSerializer.Serialize(backupJobs, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(ConfigFilePath, json);
        }

        private async Task LoadBackupStatesAsync()
        {
            if (File.Exists(StateFilePath))
            {
                var json = await File.ReadAllTextAsync(StateFilePath);
                backupStates = JsonSerializer.Deserialize<List<ModelState>>(json) ?? new List<ModelState>();
            }
        }

        private async Task SaveBackupStatesAsync()
        {
            var json = JsonSerializer.Serialize(backupStates, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(StateFilePath, json);
        }
    }
}
