using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasySave
{
    public class JsonSaver
    {


        /// <summary>
        /// save data into desired json file
        /// 
        /// mais le <T> je sais pas ce que c'est
        /// 
        /// </summary>
        public static async Task SaveJsonAsync<T>(T data, string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);                                                    //take the folder above the file
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);                                                           //create directory if it doesn't exist
            }

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });      //transform to json
            await File.WriteAllTextAsync(filePath, json);                                                       //write to desired json file
        }
    }
}
