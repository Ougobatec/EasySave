using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave
{
    public static class DataManipulation
    {
        public static void Copy(string sourceFileName, string destFileName)
        {


            // Abonnement à l'événement pour afficher les logs en temps réel


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
