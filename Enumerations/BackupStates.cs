namespace EasySave.Enumerations
{
    /// <summary>
    /// To describe global state of a backup
    /// </summary>
    public enum BackupStates
    {
        /// <summary>
        /// The backup has not been started
        /// </summary>  
        INACTIVE,

        /// <summary>
        /// The backup is currently running
        /// </summary>  
        ACTIVE,

        /// <summary>
        /// The backup has been stopped
        /// </summary>
        PAUSED,

        /// <summary>
        /// The backup is finished
        /// </summary>  
        END,

        /// <summary>
        /// The backup has failed
        /// </summary>
        ERROR
    }
}
