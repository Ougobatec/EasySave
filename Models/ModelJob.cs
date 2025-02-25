using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using EasySave.Enumerations;

namespace EasySave.Models
{
    /// <summary>
    /// Model to describe a backup job
    /// </summary>
    public class ModelJob(string name, string sourceDirectory, string targetDirectory, BackupTypes type)
    {
        /// <summary>
        /// The name of the backup
        /// </summary>
        public string Name { get; set; } = name;

        /// <summary>
        /// The source folder for the backup
        /// </summary>
        public string SourceDirectory { get; set; } = sourceDirectory;

        /// <summary>
        /// The target folder for the backup
        /// </summary>
        public string TargetDirectory { get; set; } = targetDirectory;

        /// <summary>
        /// The type of the backup
        /// </summary>
        public BackupTypes Type { get; set; } = type;

        /// <summary>
        /// The key to encrypt the backup
        /// </summary>
        public string Key { get; set; } = GenerateKey(64);

        /// <summary>
        /// The state of the backup
        /// </summary>
        public ModelState State { get; set; } = new ModelState(name);

        /// <summary>
        /// The generation of a key
        /// <param name="bits">the number of bits of the key</param>
        /// </summary>
        private static string GenerateKey(int bits)
        {
            byte[] key = new byte[bits / 8];
            using (var rng = RandomNumberGenerator.Create())    // Create a random number generator
            {
                rng.GetBytes(key);                              // Fill the key with random bytes
            }
            return Convert.ToBase64String(key);                 // Convert the key to a base64 string
        }
    }
}
