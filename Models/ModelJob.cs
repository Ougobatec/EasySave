using EasySave.Enumerations;

namespace EasySave.Models
{
    public class ModelJob
    {
        public string Name { get; set; }
        public string SourceDirectory { get; set; }
        public string TargetDirectory { get; set; }
        public BackupTypes Type { get; set; }
        public string Key { get; set; }
    }
}
