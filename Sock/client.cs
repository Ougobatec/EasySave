using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Sock
{
    class Client
    {
        public void Send()
        {
            string serverIp = "127.0.0.1"; // Adresse du serveur
            int port = 12345;

            TcpClient client = new TcpClient();
            client.Connect(serverIp, port);
            Console.WriteLine("Connecté au serveur !");

            NetworkStream stream = client.GetStream();

            // Envoyer un message
            string message = "Hello, serveur !";
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);

            // Lire la réponse du serveur
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Réponse du serveur: {response}");

            // Fermer la connexion
            client.Close();
        }
    }
}
