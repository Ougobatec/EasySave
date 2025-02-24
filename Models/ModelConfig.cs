namespace EasySave.Models
{
    /// <summary>
    /// model to describe the general setting of the application
    /// </summary>
    public class ModelConfig
    {
        /// <summary>
        /// the language of the application
        /// </summary>
        public string Language { get; set; } = "en";

        /// <summary>
        /// the format of the log
        /// </summary>
        public string LogFormat { get; set; } = "json";

        /// <summary>
        /// the maximum size of a file that can be transferred without being managed
        /// </summary>
        public int LimitSizeFile { get; set; } = 1000000;

        /// <summary>
        /// the list of all backup jobs
        /// </summary>
        public List<ModelJob> BackupJobs { get; set; } = [];

        /// <summary>
        /// the list of all priority extensions
        /// </summary>
        public List<string> PriorityExtensions { get; set; } = [];

        /// <summary>
        /// the list of all encrpyted extensions
        /// </summary>
        public List<string> EncryptedExtensions { get; set; } = [];
    }
}
