using System.Diagnostics;
using System.Linq;
using System.Windows;

public class BusinessSoftwareChecker
{
    private static readonly string[] businessSoftware = { "CalculatorApp", "calc", "mspaint" }; // Noms des logiciels métiers

    public static bool IsBusinessSoftwareRunning()
    {
        return Process.GetProcesses()
                      .Any(p => businessSoftware.Contains(p.ProcessName));
    }
}
