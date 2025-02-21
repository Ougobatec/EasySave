using System.IO;
using System.Text.Json;
using EasySave.Models;

namespace EasySave
{
    public class JsonManager
    {
        /// <summary>
        /// save data into desired json file
        /// 
        /// mais le <T> je sais pas ce que c'est
        /// 
        /// </summary>
        public static async Task SaveJsonAsync<T>(T data, string filePath)
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
        /// Load config from the config file
        /// </summary>
        /// <exception cref="Exception">creation of a new config </exception>
        public static void LoadConfig(string pathConfig)
        {
            if (File.Exists(pathConfig))
            {
                try
                {
                    var json = File.ReadAllText(pathConfig);                                            //lire tout le json du fichier
                    BackupManager.GetInstance().JsonConfig = JsonSerializer.Deserialize<ModelConfig>(json) ?? new ModelConfig();        //transform json to config data via ModelConfig class
                }
                catch (JsonException)
                {
                    BackupManager.GetInstance().JsonConfig = new ModelConfig();                                                         //je sais pas ce que c'est
                }
            }
            else
            {
                BackupManager.GetInstance().JsonConfig = new ModelConfig();                                                             //je sais pas ce que c'est
            }

            BackupManager.GetInstance().JsonConfig.Language ??= "en";
            BackupManager.GetInstance().JsonConfig.LogFormat ??= "json";
            BackupManager.GetInstance().JsonConfig.BackupJobs ??= [];
        }

        /// <summary>
        /// je sais pas ce que c'est
        /// </summary>
        /// <exception cref="Exception">creation of a new state </exception>
        public static void LoadStates(string filepath)
        {
            if (File.Exists(filepath))
            {
                try
                {
                    var json = File.ReadAllText(filepath);
                    BackupManager.GetInstance().JsonState = JsonSerializer.Deserialize<List<ModelState>>(json) ?? [];
                }
                catch (JsonException)
                {
                    BackupManager.GetInstance().JsonState = [];             //if there is a problem with the actual json create a new jsonstate
                }
            }
            else
            {
                BackupManager.GetInstance().JsonState = [];                 //if there is no json create a new jsonstate
            }
        }
    }
}
