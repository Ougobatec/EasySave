﻿namespace EasySave.Models
{


    /// <summary>
    /// Model for the configuration file
    /// </summary>
    public class ModelConfig
    {

        /// <summary>
        /// the saved language choice
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// to know if hte log is in xmlf format or json format
        /// </summary>
        public string LogFormat { get; set; }

        /// <summary>
        /// declare file size treshold to manage the limit at wich big file ar not tranfered at the same time
        /// </summary>
        public int LimitSizeFile { get; set; }

        /// <summary>
        /// list of backup jobs 
        /// </summary>
        public List<ModelJob> BackupJobs { get; set; }

        /// <summary>
        /// list of all priority extensions
        /// </summary>
        public List<string> PriorityExtensions { get; set; } = [];
    }
}
