namespace EasySave.Models
{
    public class ModelState
    {
        public string Name { get; set; } = string.Empty;
        public string SourceFilePath { get; set; } = string.Empty;
        public string TargetFilePath { get; set; } = string.Empty;
        public string State { get; set; } = "IDLE"; // Possible values: IDLE, ACTIVE, END
        public int TotalFilesToCopy { get; set; }
        public long TotalFilesSize { get; set; }
        public int NbFilesLeftToDo { get; set; }
        public int Progression { get; set; }
    }
}
