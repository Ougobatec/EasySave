using EasySave;
using System.Collections.ObjectModel;
using System.Diagnostics;

/// <summary>
/// Business software checker class to check if a business software is running
/// </summary>
public class BusinessSoftwareChecker
{
    private static BackupManager BackupManager => BackupManager.GetInstance();                                              // BackupManager instance
    private static List<string>? _businessSoftwares;
    private static List<string> BusinessSoftwares => _businessSoftwares ??= BackupManager.JsonConfig.BusinessSoftwares.ToList();

    public BusinessSoftwareChecker()
    {
        // Add business softwares from JSON
        BusinessSoftwares.AddRange(BackupManager.JsonConfig.BusinessSoftwares);
    }

    /// <summary>
    /// Check if a business software is running and return true if it is
    /// </summary>
    public static bool IsBusinessSoftwareRunning()
    {
        return Process.GetProcesses().Any(p => BusinessSoftwares.Contains(p.ProcessName));           // Check if a business software process is running
    }
}
