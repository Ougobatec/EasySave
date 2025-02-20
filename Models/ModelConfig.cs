namespace EasySave.Models
{
    public class ModelConfig
    {
        public string Language { get; set; }
        public string LogFormat { get; set; }
        public int limitSizeFile { get; set; }
        public List<ModelJob> BackupJobs { get; set; }
    }
}
