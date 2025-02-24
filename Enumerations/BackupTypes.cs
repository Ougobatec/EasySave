namespace EasySave.Enumerations
{
    /// <summary>
    /// to define the type of the backup
    /// </summary>  
    public enum BackupTypes
    {
        /// <summary>
        /// all files are being transfered
        /// </summary>  
        Full,

        /// <summary>
        /// there is comparison between the source and the target
        /// </summary>  
        Differential
    }
}
