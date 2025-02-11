using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySave.Models;
using EasySave.Enumerations;

namespace EasySave.Views
{
    public class ViewCLI
    {
        private BackupManager backupManager = new BackupManager();
        private bool exit = false;

        public void Run()
        {
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("----- MENU -----");
                Console.WriteLine("1. Add a backup job");
                Console.WriteLine("2. Update a backup job");
                Console.WriteLine("3. Execute backups");
                Console.WriteLine("4. Change settings");
                Console.WriteLine("5. Quit\n");
                Console.Write("Choose an option: ");
                var choice = Console.ReadKey(true).KeyChar;

                switch (choice)
                {
                    case '1':
                        Console.Clear();
                        AddBackupJob(backupManager).Wait();
                        break;
                    case '2':
                        Console.Clear();
                        UpdateBackupJob(backupManager).Wait();
                        break;
                    case '3':
                        Console.Clear();
                        ExecuteBackupJobs(backupManager).Wait();
                        break;
                    case '4':
                        Console.Clear();
                        ChangeSettings(backupManager).Wait();
                        break;
                    case '5':
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }


        private async Task AddBackupJob(BackupManager backupManager)
        {
            string name = GetValidInput("Job name: ", "Job name cannot be empty.");
            string sourceDirectory = GetValidInput("Source directory: ", "Source directory cannot be empty.");
            string targetDirectory = GetValidInput("Target directory: ", "Target directory cannot be empty.");
            BackupTypes type = GetValidBackupType();

            ModelJob job = new ModelJob
            {
                Name = name,
                SourceDirectory = sourceDirectory,
                TargetDirectory = targetDirectory,
                Type = type
            };

            try
            {
                await backupManager.AddBackupJobAsync(job);
                Console.WriteLine("Backup job added.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            ReturnToMenu();
        }

        private async Task UpdateBackupJob(BackupManager backupManager)
        {
            Console.WriteLine("Choose the backup job to update: ");
            DisplayBackupJobs(backupManager);
            Console.Write("Choose an option: ");
            if (int.TryParse(Console.ReadLine(), out int index) && index >= 0 && index < backupManager.config.BackupJobs.Count)
            {
                string name = GetValidInput("New job name: ", "Job name cannot be empty.");
                string sourceDirectory = GetValidInput("New source directory: ", "Source directory cannot be empty.");
                string targetDirectory = GetValidInput("New target directory: ", "Target directory cannot be empty.");
                BackupTypes type = GetValidBackupType();

                ModelJob updatedJob = new ModelJob
                {
                    Name = name,
                    SourceDirectory = sourceDirectory,
                    TargetDirectory = targetDirectory,
                    Type = type
                };

                try
                {
                    await backupManager.UpdateBackupJobAsync(index, updatedJob);
                    Console.WriteLine("Backup job updated.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Invalid index.");
            }

            ReturnToMenu();
        }

        private async Task ExecuteBackupJobs(BackupManager backupManager)
        {
            Console.WriteLine("Choose the backups to execute (separated by commas or a dash) or type 'all' to execute all:");
            DisplayBackupJobs(backupManager);
            Console.Write("Choose an option: ");
            string input = Console.ReadLine();
            if (input.ToLower() == "all")
            {
                for (int i = 0; i < backupManager.config.BackupJobs.Count; i++)
                {
                    await ExecuteBackupJobByIndex(backupManager, i);
                }
            }
            else
            {
                var indices = input.Split(',').Select(s => s.Trim()).ToList();
                foreach (var indexStr in indices)
                {
                    if (indexStr.Contains('-'))
                    {
                        var range = indexStr.Split('-').Select(s => s.Trim()).ToList();
                        if (range.Count == 2 && int.TryParse(range[0], out int start) && int.TryParse(range[1], out int end))
                        {
                            for (int i = start; i <= end; i++)
                            {
                                await ExecuteBackupJobByIndex(backupManager, i);
                            }
                        }
                    }
                    else if (int.TryParse(indexStr, out int index))
                    {
                        await ExecuteBackupJobByIndex(backupManager, index);
                    }
                    else
                    {
                        Console.WriteLine($"Invalid backup job index: {indexStr}");
                    }
                }
            }

            ReturnToMenu();
        }

        private async Task ChangeSettings(BackupManager backupManager)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("----- Change Settings -----");
                Console.WriteLine("1. Change language");
                Console.WriteLine("2. Change log format");
                Console.WriteLine("3. Save and return to menu\n");
                Console.Write("Choose an option: ");
                var choice = Console.ReadKey(true).KeyChar;

                switch (choice)
                {
                    case '1':
                        // Change language
                        Console.Write("Choose language (en/fr): ");
                        string language = Console.ReadLine().ToLower();
                        if (language == "en" || language == "fr")
                        {
                            backupManager.config.Language = language;
                            Console.WriteLine($"Language set to {language}.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid language option.");
                        }
                        break;

                    case '2':
                        // Change log format
                        Console.Write("Choose log format (json/xml): ");
                        string logFormat = Console.ReadLine().ToLower();
                        if (logFormat == "json" || logFormat == "xml")
                        {
                            backupManager.config.LogFormat = logFormat;
                            Console.WriteLine($"Log format set to {logFormat}.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid log format option.");
                        }
                        break;

                    case '3':
                        await backupManager.SaveConfigAsync();
                        Console.WriteLine("Settings saved.");
                        ReturnToMenu();
                        return;

                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
                ReturnToMenu();
            }
        }

        private async Task ExecuteBackupJobByIndex(BackupManager backupManager, int index)
        {
            if (index < 0 || index >= backupManager.config.BackupJobs.Count)
            {
                Console.WriteLine($"Invalid backup job index: {index}");
                return;
            }
            await backupManager.ExecuteBackupJobAsync(index);
        }

        private void DisplayBackupJobs(BackupManager backupManager)
        {
            Console.WriteLine("Existing backup jobs:");
            for (int i = 0; i < backupManager.config.BackupJobs.Count; i++)
            {
                var job = backupManager.config.BackupJobs[i];
                Console.WriteLine($"{i}. Name: {job.Name}, Source: {job.SourceDirectory}, Target: {job.TargetDirectory}, Type: {job.Type}");
            }
        }

        private string GetValidInput(string prompt, string errorMessage)
        {
            string input;
            do
            {
                Console.Write(prompt);
                input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine(errorMessage);
                }
            } while (string.IsNullOrWhiteSpace(input));
            return input;
        }

        private BackupTypes GetValidBackupType()
        {
            while (true)
            {
                Console.Write("Backup type (Full/Differential): ");
                string typeInput = Console.ReadLine();
                if (Enum.TryParse(typeof(BackupTypes), typeInput, true, out var parsedType))
                {
                    return (BackupTypes)parsedType;
                }
                else
                {
                    Console.WriteLine("Invalid backup type.");
                }
            }
        }

        private void ReturnToMenu()
        {
            Console.WriteLine();
            Console.WriteLine("----- Press Enter to continue. -----");
            Console.ReadLine();
        }
    }
}
