using System.Security.RightsManagement;

namespace EasySave.Models
{

    /// <summary>
    /// model to describe the state of a backup
    /// 
    /// 
    /// je sais pas ce que c'est ILogModel
    /// 
    /// 
    /// 
    /// </summary>
    public class ModelState
    {
        public string Name { get; set; }
        /// <summary>
        /// esque c'est le chemin de source de la backup ou de un fichier ?
        /// 
        /// 
        /// </summary>
        public string SourceFilePath { get; set; } = string.Empty;

        /// <summary>
        /// esque c'est le chemin de destination de la backup ou de un fichier ?
        /// 
        /// 
        /// </summary>
        public string TargetFilePath { get; set; } = string.Empty;

        /// <summary>
        /// the actual state of the backup with IDLE defautl value
        /// 
        /// 
        /// </summary>
        public string State { get; set; } = "IDLE";

        /// <summary>
        /// esque c'est la taille des fichiers à copier ?
        /// 
        /// 
        /// </summary>
        public int TotalFilesToCopy { get; set; }

        /// <summary>
        /// esque c'est la taille des fichiers à copier ?
        /// 
        /// 
        /// </summary>
        public long TotalFilesSize { get; set; }

        /// <summary>
        /// how many file are left to do in this backup
        /// 
        /// 
        /// </summary>
        public int NbFilesLeftToDo { get; set; }


        private int progression;    
        /// <summary>
        /// je sais pas ce que c'est
        /// 
        /// 
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

        public event EventHandler? OnProgressChange;

        public ModelState(string name)
        {
            Name = name;
        }
    }
}
