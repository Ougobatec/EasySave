﻿using System.ComponentModel;
using System.Security.RightsManagement;
using EasySave.Enumerations;

namespace EasySave.Models
{
    /// <summary>
    /// Model to describe the state of a backup
    /// </summary>
    public class ModelState(string name) : INotifyPropertyChanged
    {
        /// <summary>
        /// The name of the backup
        /// </summary>
        public string Name { get; set; } = name;

        /// <summary>
        /// The source of the moving file
        /// </summary>
        public string SourceFilePath { get; set; } = string.Empty;

        /// <summary>
        /// The destination of the moving file
        /// </summary>
        public string TargetFilePath { get; set; } = string.Empty;

        /// <summary>
        /// The last directory where the backup was saved
        /// </summary>
        public string LastSaveDirectory { get; set; } = string.Empty;

        /// <summary>
        /// The state of the backup
        /// </summary>
        private BackupStates _state = BackupStates.READY;
        public BackupStates State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged(nameof(State));
                }
            }
        }

        /// <summary>
        /// The number of files to transfer
        /// </summary>
        public int TotalFilesToCopy { get; set; }

        /// <summary>
        /// The size of the files to transfer
        /// </summary>
        public long TotalFilesSize { get; set; }

        /// <summary>
        /// The number of files left to transfer
        /// </summary>
        public int NbFilesLeftToDo { get; set; }

        /// <summary>
        /// List of files remaining to copy
        /// </summary>
        public List<string> RemainingFiles { get; set; } = new List<string>();

        /// <summary>
        /// The progression of the backup
        /// </summary>
        private int _progression;
        public int Progression
        {
            get { return _progression; }
            set
            {
                if (_progression != value)
                {
                    _progression = value;
                    OnPropertyChanged(nameof(Progression));
                }
            }
        }

        /// <summary>
        /// The event to notify the change of a property
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notify the change of a property
        /// <param name="propertyName">the name of the property</param>
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
