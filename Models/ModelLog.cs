using Logger;

namespace EasySave.Models
{
    /// <summary>
    /// Model to describe a log
    /// </summary>
    public class ModelLog(string backupName = "", string source = "", string destination = "", long size = 0, TimeSpan encryptionTime = default, TimeSpan transferTime = default) : ILogModel
    {
        /// <summary>
        /// The time at the log creation
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// The name of the backup
        /// </summary>  
        public string BackupName { get; set; } = backupName;

        /// <summary>
        /// The source of the moving file
        /// </summary>  
        public string Source { get; set; } = source;

        /// <summary>
        /// The destination of the moving file
        /// </summary>  
        public string Destination { get; set; } = destination;

        /// <summary>
        /// The size of the moving file
        /// </summary>  
        public long Size { get; set; } = size;

        /// <summary>
        /// The time taken to encrypt the file
        /// </summary>
        public TimeSpan EncryptionTime { get; set; } = encryptionTime;

        /// <summary>
        /// The time taken to move the file
        /// </summary>
        public TimeSpan TransferTime { get; set; } = transferTime;
    }
}
