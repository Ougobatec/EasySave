using System.IO;
using System.Text.Json;

namespace EasySave
{
    /// <summary>
    /// Json manager class to load and save json files
    /// </summary>
    public class JsonManager
    {
        /// <summary>
        /// Load data from desired json file into desired object
        /// <param name="filePath">Json file path</param>
        /// <param name="defaultValue">Default value if file does not exist</param>
        /// </summary>
        public static T LoadJson<T>(string filePath, T defaultValue) where T : new()
        {
            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);                          // Read json from file
                    return JsonSerializer.Deserialize<T>(json) ?? defaultValue;     // Deserialize json to object
                }
                catch (JsonException)
                {
                    return defaultValue;                                            // Return default value if json is invalid
                }
            }
            return defaultValue;                                                    // Return default value if file does not exist
        }

        /// <summary>
        /// Save data into desired json file from desired object
        /// <param name="data">Data to save</param>
        /// <param name="filePath">Json file path</param>
        /// </summary>
        public static async Task SaveJsonAsync<T>(T data, string filePath)
        {
            string? directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);                                                       // Create directory if not exists
            }

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });      // Serialize object to json
            await File.WriteAllTextAsync(filePath, json);                                                       // Write json to file
        }
    }
}
