namespace EasySave.Enumerations
{
    /// <summary>
    /// To define the type of the backup
    /// </summary>  
    public enum BackupTypes
    {
        /// <summary>
        /// All files are being transfered
        /// </summary>  
        Full,

        /// <summary>
        /// There is comparison between the source and the target
        /// </summary>  
        Differential
    }
}
