using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
        public string FileName;

        public Logger(string fileName)
        {
            FileName = fileName;
        }

        public void Debug(string message)
        {
            Log("debug", message);
        }

        public void Trace(string message)
        {
            Log("trace", message);
        }

        public void Warning(string message)
        {
            Log("warning", message);
        }

        public void Error(string message)
        {
            Log("error", message);
        }

        public  void Info(string messagelog)
        {
             Log("info", messagelog);
        }

        private void Log(string logType, string message)
        {
            var lignejson = new boutdejson
            {
                date = DateTime.Now.ToString(),
                typedelog = logType,
                message = message
            };

            try
            {
                string jsonString = JsonSerializer.Serialize(lignejson);
                File.AppendAllText(FileName, jsonString +","+ Environment.NewLine);
                Console.WriteLine(jsonString);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"An error occurred while writing to the file: {ex.Message}");
            }
        }

     
        
    }

}







//Debug
//    Trace²
//    warining
//    error 

//    info


//    temp type de log

//    message

//    en json