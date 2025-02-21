namespace EasySave.Enumerations
{


    /// <summary>
    /// to describe the format of the log  
    /// </summary>  
    public enum LogFormats
    {

        /// <summary>
        /// JSON log saved like this
        /// 
        /// <example> 
        ///   {
        /// "Timestamp": "2025-02-17T11:56:21.6562304+01:00",
        /// BackupName": "nameofhtebackup",
        /// Source": "source path in UNC",
        /// Destination": "destination path in UNC",
        /// Size": 1624144,
        /// "CryptedTime": 0,
        /// "TransfertTime": "00:00:00.0288453"
        ///},
        /// </example>
        /// 
        /// </summary>  
        Json,
        Xml
    }
}