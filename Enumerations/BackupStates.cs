namespace EasySave.Enumerations
{
    /// <summary>
    /// to describe global state of a backup
    /// </summary>
    public enum BackupStates
    {
        /// <summary>
        /// the backup has not been started
        /// </summary>  
        INACTIVE,

        /// <summary>
        /// the backup is currently running
        /// </summary>  
        ACTIVE,

        /// <summary>
        /// the backup has been stopped
        /// </summary>
        PAUSED,

        /// <summary>
        /// the backup is finished
        /// </summary>  
        END,

        /// <summary>
        /// the backup has failed
        /// </summary>
        ERROR
    }
}
