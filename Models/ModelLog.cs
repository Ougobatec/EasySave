using Logger;

namespace EasySave.Models
{
    public class ModelLog : ILogModel
    {
        public DateTime Timestamp { get; set; }
        public string BackupName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public long Size { get; set; }
        public TimeSpan TransfertTime { get; set; }
    }
}
