//using EasySave.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;

//namespace EasySave
//{
//    internal class temp
//    {

//        private void LoadConfig(string ConfigFilePath)
//        {
//            if (File.Exists(ConfigFilePath))
//            {
//                try
//                {
//                    var json = File.ReadAllText(ConfigFilePath);                                            //lire tout le json du fichier
//                    JsonConfig = JsonSerializer.Deserialize<ModelConfig>(json) ?? new ModelConfig();        //transform json to config data via ModelConfig class
//                }
//                catch (JsonException)
//                {
//                    JsonConfig = new ModelConfig();                                                         //je sais pas ce que c'est
//                }
//            }
//            else
//            {
//                JsonConfig = new ModelConfig();                                                             //je sais pas ce que c'est
//            }

//            JsonConfig.Language ??= "en";
//            JsonConfig.LogFormat ??= "json";
//            JsonConfig.BackupJobs ??= [];
//        }


//    }
//}
