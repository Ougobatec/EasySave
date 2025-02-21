namespace EasySave.Enumerations
{


    /// <summary>
    /// to describe global state of a backup
    /// </summary>
    public enum BackupStates
    {

        /// <summary>
        /// nothing is happening at the moment
        /// </summary>  
        IDLE,

        /// <summary>
        /// file are being transfered
        /// </summary>  
        ACTIVE,

        /// <summary>
        /// the backup is finished
        /// </summary>  
        END
    }
}
