using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using EasySave.Enumerations;

namespace EasySave.Models
{
    /// <summary>
    /// setting for a backup job 
    /// </summary>
    /// <remarks>
    /// constructor for the backup job
    /// </remarks>
    public class ModelJob(string name, string sourceDirectory, string targetDirectory, BackupTypes type)
    {
        /// <summary>
        /// name of the backup
        /// </summary>
        public string Name { get; set; } = name;

        /// <summary>
        /// source folder for the backup
        /// </summary>
        public string SourceDirectory { get; set; } = sourceDirectory;

        /// <summary>
        /// destination folder for the backup
        /// </summary>
        public string TargetDirectory { get; set; } = targetDirectory;

        /// <summary>
        /// the type of the backup: diferential or full
        /// </summary>
        public BackupTypes Type { get; set; } = type;

        /// <summary>
        /// encryption key
        /// </summary>
        public string Key { get; set; } = GenerateKey(64);

        /// <summary>
        /// the state of the backup
        /// </summary>
        public ModelState State { get; set; } = new ModelState(name);

        /// <summary>
        /// create a cryptographic key via of the desired length via a random number generator
        /// </summary>
        private static string GenerateKey(int bits)
        {
            byte[] key = new byte[bits / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return Convert.ToBase64String(key);//transform to base64
        }
    }
}
