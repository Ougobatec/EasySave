using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySave.Interface;
namespace EasySave
{
    public class BackupManager
    {
        List<IBackupJob> _backup = new List<IBackupJob> ();
        public BackupManager() { }
        public void AddBackup(IBackupJob iBackupJob) 
        {
            Console.ReadLine("");
            _backup.Add(iBackupJob);
            Console.WriteLine($"AddBackup : {iBackupJob}");
        }
        public void ExecuteBackup(string name) { }
    }
}
