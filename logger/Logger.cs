using System;
using System.IO;
using System.Text.Json;

namespace EasySave
{
    public class boutdejson
    {
        public string date { get; set; }
        public string typedelog { get; set; }
        public string message { get; set; }
    }

    public class Logger
    {
        public string FileName { get; private set; }

        // Définition de l'événement
        public event EventHandler<string> LogWritten;

        public Logger(string fileName)
        {
            FileName = fileName;
        }

        public void Debug(string message) => Log("debug", message);
        public void Trace(string message) => Log("trace", message);
        public void Warning(string message) => Log("warning", message);
        public void Error(string message) => Log("error", message);
        public void Info(string message) => Log("info", message);

        private void Log(string logType, string message)
        {
            var logEntry = new boutdejson
            {
                date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                typedelog = logType,
                message = message
            };

            try
            {
                string jsonString = JsonSerializer.Serialize(logEntry);
                File.AppendAllText(FileName, jsonString + "," + Environment.NewLine);

                // Déclencher l'événement pour notifier les abonnés
                LogWritten?.Invoke(this, jsonString);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Erreur lors de l'écriture du fichier de log: {ex.Message}");
            }
        }
    }
}
