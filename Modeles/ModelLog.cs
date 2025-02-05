//using System.Collections;
namespace EasySave.Modeles
{
    public class ModelLog
    {
        public DateTime Timestamp;
        public string BackupName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public long FileSize;
        public TimeSpan TransfertTime;
    }
}
