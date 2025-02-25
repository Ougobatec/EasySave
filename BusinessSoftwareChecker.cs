namespace EasySave
{
    using System.Diagnostics;

    /// <summary>
    /// Business software checker class to check if a business software is running
    /// </summary>
    public class BusinessSoftwareChecker
    {
        private static BackupManager BackupManager => BackupManager.GetInstance();                                                  // BackupManager instance
        private static List<string>? _businessSoftwares;                                                                            // Business softwares list
        private static List<string> BusinessSoftwares => _businessSoftwares ??= [.. BackupManager.JsonConfig.BusinessSoftwares];    // Business softwares list

        /// <summary>
        /// Business software checker constructor to add business softwares from JSON
        /// </summary>
        public BusinessSoftwareChecker()
        {
            BusinessSoftwares.AddRange(BackupManager.JsonConfig.BusinessSoftwares);                 // Add business softwares from JSON
        }

        /// <summary>
        /// Check if a business software is running and return true if it is
        /// </summary>
        public static bool IsBusinessSoftwareRunning()
        {
            return Process.GetProcesses().Any(p => BusinessSoftwares.Contains(p.ProcessName));      // Check if a business software process is running
        }
    }
}
