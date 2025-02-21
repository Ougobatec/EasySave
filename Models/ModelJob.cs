using EasySave.Enumerations;

namespace EasySave.Models
{

    /// <summary>
    /// setting for a backup job 
    /// </summary>
    public class ModelJob
    {

        /// <summary>
        /// name of the backup
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// source folder for the backup
        /// </summary>
        public string SourceDirectory { get; set; }

        /// <summary>
        /// destination folder for the backup
        /// </summary>
        public string TargetDirectory { get; set; }

        /// <summary>
        /// the type of the backup: diferential or full
        /// </summary>
        public BackupTypes Type { get; set; }

        /// <summary>
        /// encryption key
        /// </summary>
        public string Key { get; set; }
    }
}
