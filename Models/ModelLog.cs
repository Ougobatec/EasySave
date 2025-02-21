using Logger;

namespace EasySave.Models
{


    /// <summary>
    /// model of log
    /// 
    /// 
    /// je sais pas ce que c'est ILogModel
    /// 
    /// 
    /// 
    /// </summary>
    public class ModelLog : ILogModel
    {

        /// <summary>
        /// the time at the log creation
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// the name og the actuak backup
        /// </summary>  
        public string BackupName { get; set; } = string.Empty;

        /// <summary>
        /// the source of the moving file
        /// </summary>  
        public string Source { get; set; } = string.Empty;
        /// <summary>
        /// the destination of the moving file
        /// </summary>  
        public string Destination { get; set; } = string.Empty;
        /// <summary>
        /// the size of te moving file in Bytes
        /// </summary>  
        public long Size { get; set; }

        /// <summary>
        /// time takken to encrypt the file
        /// </summary>
        public TimeSpan EncryptionTime { get; set; }

        /// <summary>
        /// time takken to transfert the file
        /// </summary>
        public TimeSpan TransfertTime { get; set; }
    }
}
