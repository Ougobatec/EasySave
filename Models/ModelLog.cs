using Logger;

namespace EasySave.Models
{
    /// <summary>
    /// model to describe a log
    /// </summary>
    public class ModelLog(string backupName = "", string source = "", string destination = "", long size = 0, TimeSpan encryptionTime = default, TimeSpan transferTime = default) : ILogModel
    {
        /// <summary>
        /// the time at the log creation
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// the name of the backup
        /// </summary>  
        public string BackupName { get; set; } = backupName;

        /// <summary>
        /// the source of the moving file
        /// </summary>  
        public string Source { get; set; } = source;

        /// <summary>
        /// the destination of the moving file
        /// </summary>  
        public string Destination { get; set; } = destination;

        /// <summary>
        /// the size of the moving file
        /// </summary>  
        public long Size { get; set; } = size;

        /// <summary>
        /// the time taken to encrypt the file
        /// </summary>
        public TimeSpan EncryptionTime { get; set; } = encryptionTime;

        /// <summary>
        /// the time taken to move the file
        /// </summary>
        public TimeSpan TransferTime { get; set; } = transferTime;
    }
}
