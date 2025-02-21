using System.Diagnostics;

/// <summary>
/// Class to check business software
/// </summary>
public class BusinessSoftwareChecker
{
    private static readonly string[] businessSoftware = { "CalculatorApp", "calc", "mspaint" }; // Name of business softwares

    /// <summary>
    /// Checks if a business software is running and return true or false
    /// </summary>
    public static bool IsBusinessSoftwareRunning()
    {
        return Process.GetProcesses()
                      .Any(p => businessSoftware.Contains(p.ProcessName));
    }
}