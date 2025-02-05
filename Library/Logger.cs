using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EasySave.Library
{
    public class Logger
    {
        private string LogFileName { get; set; }
        public Logger(string logFileName)
        {
            LogFileName = logFileName;            
        }

        public void Debug(string message) 
        {
            File.WriteAllText(LogFileName, $"DEBUG : {message}");
        }

        public void Info(string message) 
        {
            File.WriteAllText(LogFileName, $"INFO : {message}");
        }
        public void Warn(string message) { }  
        public void Error(string message) { }
        public void Fatal(string message) { }


    }
}
