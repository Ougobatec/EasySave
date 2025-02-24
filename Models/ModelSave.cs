namespace EasySave.Models
{
    /// <summary>
    /// Model for a save folder
    /// </summary>
    public class ModelSave(string name, string type, long size, DateTime date)
    {
        /// <summary>
        /// the Name of the save (name of the folder)
        /// </summary>
        public string Name { get; set; } = name;

        /// <summary>
        /// the Type of the save (differential or full)
        /// </summary>
        public string Type { get; set; } = type;

        /// <summary>
        /// the size of the save (all files)
        /// </summary>
        public long Size { get; set; } = size;

        /// <summary>
        /// the time at the save creation
        /// </summary>
        public DateTime Date { get; set; } = date;
    }
}
