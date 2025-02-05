using EasySave.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySave.Interface;

namespace EasySave
{
    public class FullBackupJob : IBackupJob
    {

        public string Name { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public void Execute() 
        {
            Console.WriteLine("FullBackupJob");
        }
    }
}
