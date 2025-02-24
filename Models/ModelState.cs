using System.Security.RightsManagement;

namespace EasySave.Models
{
    /// <summary>
    /// model to describe the state of a backup
    /// </summary>
    public class ModelState(string name)
    {
        /// <summary>
        /// the name of the backup
        /// </summary>
        public string Name { get; set; } = name;

        /// <summary>
        /// the source of the moving file
        /// </summary>
        public string SourceFilePath { get; set; } = string.Empty;

        /// <summary>
        /// the destination of the moving file
        /// </summary>
        public string TargetFilePath { get; set; } = string.Empty;

        /// <summary>
        /// the state of the backup
        /// </summary>
        public string State { get; set; } = "IDLE";

        /// <summary>
        /// the number of files to transfer
        /// </summary>
        public int TotalFilesToCopy { get; set; }

        /// <summary>
        /// the size of the files to transfer
        /// </summary>
        public long TotalFilesSize { get; set; }

        /// <summary>
        /// the number of files left to transfer
        /// </summary>
        public int NbFilesLeftToDo { get; set; }

        /// <summary>
        /// the progression of the backup
        /// </summary>
        private int progression;

        /// <summary>
        /// the progression of the backup
        /// </summary>
        public int Progression 
        {
            get => progression;
            set
            {
                progression = value;
                OnProgressChange?.Invoke(this, EventArgs.Empty);
            }
            
        }

        /// <summary>
        /// event to trigger when the progression of the backup changes
        /// </summary>
        public event EventHandler? OnProgressChange;
    }
}
