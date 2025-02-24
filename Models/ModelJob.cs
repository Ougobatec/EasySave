using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using EasySave.Enumerations;

namespace EasySave.Models
{
    /// <summary>
    /// model to describe a backup job
    /// </summary>
    public class ModelJob(string name, string sourceDirectory, string targetDirectory, BackupTypes type)
    {
        /// <summary>
        /// the name of the backup
        /// </summary>
        public string Name { get; set; } = name;

        /// <summary>
        /// source folder for the backup
        /// </summary>
        public string SourceDirectory { get; set; } = sourceDirectory;

        /// <summary>
        /// the target folder for the backup
        /// </summary>
        public string TargetDirectory { get; set; } = targetDirectory;

        /// <summary>
        /// the type of the backup
        /// </summary>
        public BackupTypes Type { get; set; } = type;

        /// <summary>
        /// the key to encrypt the backup
        /// </summary>
        public string Key { get; set; } = GenerateKey(64);

        /// <summary>
        /// the state of the backup
        /// </summary>
        public ModelState State { get; set; } = new ModelState(name);

        /// <summary>
        /// the generation of a key
        /// <param name="bits">the number of bits of the key</param>
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
