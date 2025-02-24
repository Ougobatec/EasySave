namespace EasySave.Models
{
    /// <summary>
    /// Model for a save folder
    /// </summary>
    public class ModelSave(string name, string type, long size, DateTime date)
    {
        /// <summary>
        /// The Name of the save (name of the folder)
        /// </summary>
        public string Name { get; set; } = name;

        /// <summary>
        /// The Type of the save (differential or full)
        /// </summary>
        public string Type { get; set; } = type;

        /// <summary>
        /// The size of the save (all files)
        /// </summary>
        public long Size { get; set; } = size;

        /// <summary>
        /// The time at the save creation
        /// </summary>
        public DateTime Date { get; set; } = date;
    }
}
