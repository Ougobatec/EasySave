using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.DataManipulation
{
    internal class DataManipulation
    {
        public static void Copy(string sourceFileName, string destFileName)
        {
            if (File.Exists(sourceFileName))
            {
                File.Copy(sourceFileName, destFileName, true);
            }
            else
            {
                throw new FileNotFoundException("Source file not found.", sourceFileName);
            }
        }
    }

}

