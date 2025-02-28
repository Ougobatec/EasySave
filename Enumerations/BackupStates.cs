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
        READY,

        /// <summary>
        /// The backup is currently running
        /// </summary>  
        ACTIVE,

        /// <summary>
        /// The backup has been stopped
        /// </summary>
        PAUSED
    }
}
