using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.DataManipulation
{
    public class Testdelegate
    {

        public delegate void MonDelegate(string message);

        public void AfficherMessage(MonDelegate callback)
        {
            callback("Ceci est un callback !");
        }



        public void DireQuelqueChose(string msg)
        {
            Console.WriteLine(msg);
        }

        public void DireBonjour()
        {
            Console.WriteLine("Salut !");
        }

    }
}
