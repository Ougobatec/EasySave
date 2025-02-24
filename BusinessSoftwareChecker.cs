using System.Diagnostics;

/// <summary>
/// Business software checker class to check if a business software is running
/// </summary>
public class BusinessSoftwareChecker
{
    private static readonly string[] businessSoftware = { "CalculatorApp", "calc", "mspaint" };     // Business software process names

    /// <summary>
    /// Check if a business software is running and return true if it is
    /// </summary>
    public static bool IsBusinessSoftwareRunning()
    {
        return Process.GetProcesses().Any(p => businessSoftware.Contains(p.ProcessName));           // Check if a business software process is running
    }
}
