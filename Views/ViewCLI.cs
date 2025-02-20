//using System.Resources;
//using EasySave.Enumerations;
//using EasySave.Models;

//namespace EasySave.Views
//{
//    public class ViewCLI
//    {
//        private static ResourceManager ResourceManager => BackupManager.GetInstance().resourceManager;
//        private bool exit = false;

//        public void Run()
//        {
//            while (!exit)
//            {
//                Console.Clear();
//                Console.WriteLine(ResourceManager.GetString("Title_Menu"));
//                Console.WriteLine("1. " + ResourceManager.GetString("Menu_AddBackupJob"));
//                Console.WriteLine("2. " + ResourceManager.GetString("Menu_UpdateBackupJob"));
//                Console.WriteLine("3. " + ResourceManager.GetString("Menu_ExecuteBackups"));
//                Console.WriteLine("4. " + ResourceManager.GetString("Menu_DeleteBackupJob"));
//                Console.WriteLine("5. " + ResourceManager.GetString("Menu_ChangeSettings"));
//                Console.WriteLine("6. " + ResourceManager.GetString("Menu_Quit"));
//                Console.Write("\n" + ResourceManager.GetString("Prompt_ChooseOption"));
//                var choice = Console.ReadKey(true).KeyChar;

//                switch (choice)
//                {
//                    case '1':
//                        Console.Clear();
//                        AddBackupJob().Wait();
//                        break;
//                    case '2':
//                        Console.Clear();
//                        UpdateBackupJob().Wait();
//                        break;
//                    case '3':
//                        Console.Clear();
//                        ExecuteBackupJobs().Wait();
//                        break;
//                    case '4':
//                        Console.Clear();
//                        DeleteBackupJob().Wait();
//                        break;
//                    case '5':
//                        Console.Clear();
//                        ChangeSettings().Wait();
//                        break;
//                    case '6':
//                        exit = true;
//                        break;
//                    default:
//                        break;
//                }
//            }
//        }

//        private static async Task AddBackupJob()
//        {
//            Console.WriteLine(ResourceManager.GetString("Title_ChooseBackupJobToAdd"));
//            string name = GetValidInput(ResourceManager.GetString("Prompt_JobName"), ResourceManager.GetString("Error_Empty"));
//            string sourceDirectory = GetValidInput(ResourceManager.GetString("Prompt_SourceDirectory"), ResourceManager.GetString("Error_Empty"));
//            string targetDirectory = GetValidInput(ResourceManager.GetString("Prompt_TargetDirectory"), ResourceManager.GetString("Error_Empty"));
//            BackupTypes type = GetValidBackupType();

//            ModelJob job = new()
//            {
//                Name = name,
//                SourceDirectory = sourceDirectory,
//                TargetDirectory = targetDirectory,
//                Type = type
//            };

//            try
//            {
//                await BackupManager.GetInstance().AddBackupJobAsync(job);
//                Console.WriteLine(ResourceManager.GetString("Message_BackupJobAdded"), job.Name);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//            }

//            ReturnToMenu();
//        }

//        private static async Task UpdateBackupJob()
//        {
//            Console.WriteLine(ResourceManager.GetString("Title_ChooseBackupJobToUpdate"));
//            DisplayBackupJobs();
//            Console.Write("\n" + ResourceManager.GetString("Prompt_ChooseOption"));
//            var input = Console.ReadLine();
//            if (int.TryParse(input, out int index) && index >= 0 && index < BackupManager.GetInstance().JsonConfig.BackupJobs.Count)
//            {
//                string name = GetValidInput(ResourceManager.GetString("Prompt_JobName"), ResourceManager.GetString("Error_Empty"));
//                string sourceDirectory = GetValidInput(ResourceManager.GetString("Prompt_SourceDirectory"), ResourceManager.GetString("Error_Empty"));
//                string targetDirectory = GetValidInput(ResourceManager.GetString("Prompt_TargetDirectory"), ResourceManager.GetString("Error_Empty"));
//                BackupTypes type = GetValidBackupType();

//                ModelJob newdJob = new()
//                {
//                    Name = name,
//                    SourceDirectory = sourceDirectory,
//                    TargetDirectory = targetDirectory,
//                    Type = type
//                };

//                try
//                {
//                    await BackupManager.GetInstance().UpdateBackupJobAsync(newdJob, index);
//                    Console.WriteLine(ResourceManager.GetString("Message_BackupJobUpdated"), newdJob.Name);
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine(ex.Message);
//                }
//            }
//            else
//            {
//                Console.WriteLine(ResourceManager.GetString("Error_InvalidIndex"), input);
//            }

//            ReturnToMenu();
//        }

//        private static async Task ExecuteBackupJobs()
//        {
//            Console.WriteLine(ResourceManager.GetString("Title_ChooseBackupJobsToExecute"));
//            DisplayBackupJobs();
//            Console.Write("\n" + ResourceManager.GetString("Prompt_ChooseOption"));
//            var input = Console.ReadLine();
//            if (input.Equals("all", StringComparison.CurrentCultureIgnoreCase))
//            {
//                for (int i = 0; i < BackupManager.GetInstance().JsonConfig.BackupJobs.Count; i++)
//                {
//                    await ExecuteBackupJobByIndex(i);
//                }
//            }
//            else
//            {
//                var indices = input.Split(',').Select(s => s.Trim()).ToList();
//                foreach (var indexStr in indices)
//                {
//                    if (indexStr.Contains('-'))
//                    {
//                        var range = indexStr.Split('-').Select(s => s.Trim()).ToList();
//                        if (range.Count == 2 && int.TryParse(range[0], out int start) && int.TryParse(range[1], out int end))
//                        {
//                            for (int i = start; i <= end; i++)
//                            {
//                                await ExecuteBackupJobByIndex(i);
//                            }
//                        }
//                    }
//                    else if (int.TryParse(indexStr, out int index))
//                    {
//                        await ExecuteBackupJobByIndex(index);
//                    }
//                    else
//                    {
//                        Console.WriteLine(string.Format(ResourceManager.GetString("Error_InvalidIndex"), indexStr));
//                    }
//                }
//            }

//            ReturnToMenu();
//        }

//        private static async Task DeleteBackupJob()
//        {
//            Console.WriteLine(ResourceManager.GetString("Title_ChooseBackupJobToDelete"));
//            DisplayBackupJobs();
//            Console.Write("\n" + ResourceManager.GetString("Prompt_ChooseOption"));
//            var input = Console.ReadLine();
//            if (int.TryParse(input, out int index) && index >= 0 && index < BackupManager.GetInstance().JsonConfig.BackupJobs.Count)
//            {
//                try
//                {
//                    await BackupManager.GetInstance().DeleteBackupJobAsync(index);
//                    Console.WriteLine(ResourceManager.GetString("Message_BackupJobDeleted"), index);
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine(ex.Message);
//                }
//            }
//            else
//            {
//                Console.WriteLine(ResourceManager.GetString("Error_InvalidIndex"), input);
//            }

//            ReturnToMenu();
//        }

//        private static async Task ChangeSettings()
//        {
//            string language = null;
//            string logFormat = null;

//            while (true)
//            {
//                Console.Clear();
//                Console.WriteLine(ResourceManager.GetString("Title_ChooseSettingToChange"));
//                Console.WriteLine("1. " + ResourceManager.GetString("Menu_ChangeLanguage"));
//                Console.WriteLine("2. " + ResourceManager.GetString("Menu_ChangeLogFormat"));
//                Console.WriteLine("3. " + ResourceManager.GetString("Menu_SaveAndReturn"));
//                Console.Write("\n" + ResourceManager.GetString("Prompt_ChooseOption"));
//                var choice = Console.ReadKey(true).KeyChar;

//                switch (choice)
//                {
//                    case '1':
//                        Console.Write(ResourceManager.GetString("Prompt_ChooseLanguage"));
//                        language = Console.ReadLine().ToLower();
//                        if (language != "en" && language != "fr")
//                        {
//                            Console.WriteLine(ResourceManager.GetString("Error_InvalidOption"));
//                            language = null;
//                        }
//                        ReturnToMenu();
//                        break;

//                    case '2':
//                        Console.Write(ResourceManager.GetString("Prompt_ChooseLogFormat"));
//                        logFormat = Console.ReadLine().ToLower();
//                        if (logFormat != "json" && logFormat != "xml")
//                        {
//                            Console.WriteLine(ResourceManager.GetString("Error_InvalidOption"));
//                            logFormat = null;
//                        }
//                        ReturnToMenu();
//                        break;

//                    case '3':
//                        await BackupManager.GetInstance().ChangeSettingsAsync(language, logFormat);
//                        Console.WriteLine(ResourceManager.GetString("Message_SettingsSaved"));
//                        ReturnToMenu();
//                        return;

//                    default:
//                        Console.WriteLine(ResourceManager.GetString("Error_InvalidOption"));
//                        break;
//                }
//            }
//        }

//        private static async Task ExecuteBackupJobByIndex(int index)
//        {
//            if (index < 0 || index >= BackupManager.GetInstance().JsonConfig.BackupJobs.Count)
//            {
//                Console.WriteLine(string.Format(ResourceManager.GetString("Error_InvalidIndex"), index));
//                return;
//            }
//            await BackupManager.GetInstance().ExecuteBackupJobAsync(index);
//        }

//        private static void DisplayBackupJobs()
//        {
//            Console.WriteLine();
//            for (int i = 0; i < BackupManager.GetInstance().JsonConfig.BackupJobs.Count; i++)
//            {
//                var job = BackupManager.GetInstance().JsonConfig.BackupJobs[i];
//                Console.WriteLine(string.Format(ResourceManager.GetString("Message_BackupJobDetails"), i, job.Name, job.SourceDirectory, job.TargetDirectory, job.Type));
//            }
//        }

//        private static string GetValidInput(string prompt, string errorMessage)
//        {
//            string input;
//            do
//            {
//                Console.Write(prompt);
//                input = Console.ReadLine();
//                if (string.IsNullOrWhiteSpace(input))
//                {
//                    Console.WriteLine(errorMessage);
//                }
//            } while (string.IsNullOrWhiteSpace(input));
//            return input;
//        }

//        private static BackupTypes GetValidBackupType()
//        {
//            while (true)
//            {
//                Console.Write(ResourceManager.GetString("Prompt_BackupType"));
//                string typeInput = Console.ReadLine();
//                if (Enum.TryParse(typeof(BackupTypes), typeInput, true, out var parsedType))
//                {
//                    return (BackupTypes)parsedType;
//                }
//                else
//                {
//                    Console.WriteLine(ResourceManager.GetString("Error_InvalidBackupType"));
//                }
//            }
//        }

//        private static void ReturnToMenu()
//        {
//            Console.WriteLine("\n" + ResourceManager.GetString("Message_PressEnterToContinue"));
//            Console.ReadLine();
//        }
//    }
//}
