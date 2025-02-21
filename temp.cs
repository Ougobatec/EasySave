//using EasySave.Models;
//using Logger;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;

//namespace EasySave
//{
//    internal class temp
//    {

//        public async Task AddBackupJobAsync(ModelJob job)
//        {
//            if (JsonConfig.BackupJobs.Any(b => b.Name.Equals(job.Name, StringComparison.OrdinalIgnoreCase)))
//            {
//                throw new Exception("Message_NameExists");
//            }

//            job.Key = GenerateKey(64);
//            JsonConfig.BackupJobs.Add(job);
//            JsonState.Add(new ModelState { Name = job.Name });
//            await SaveJsonAsync(JsonConfig, ConfigFilePath);
//            await SaveJsonAsync(JsonState, StateFilePath);
//        }

//    }
//}
