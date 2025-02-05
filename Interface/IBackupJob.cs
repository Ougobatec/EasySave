using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Interface
{
    public interface IBackupJob
    {
        public string Name { get; }
        public string SourcePath {  get; }
        public string DestinationPath { get; }
        public void Execute();
    }
}
