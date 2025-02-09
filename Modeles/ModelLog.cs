using Logger;

namespace EasySave.Modeles
{
    public class ModelLog : ILogModel
    {
        public DateTime Timestamp { get; set; }
        public string BackupName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public TimeSpan TransfertTime { get; set; }
    }
}
