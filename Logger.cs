using System.Collections;

public class Logger
{
    private string Logger_LogFile = string.Empty;
    private static Logger? Logger_Instance = null; // Utilisation de ? pour permettre la valeur null
    private List<ModelLog> Logger_Logs = new List<ModelLog>();

    private Logger()
    {
    }

    public static Logger GetInstance()
    {
        if (Logger_Instance == null)
        {
            Logger_Instance = new Logger();
        }
        return Logger_Instance;
    }

    public void Log(string backupName, string source, string destination, long size, TimeSpan duration)
    {
    }

    private void LoadLogs()
    {
    }

    private void SaveLogs()
    {
    }

    private class ModelLog
    {
        public DateTime Timestamp;
        public string BackupName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public long FileSize;
        public TimeSpan TransfertTime;
    }
}