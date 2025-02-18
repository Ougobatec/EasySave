using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Sock
{
    class Server
    {


        public void Demarage()
        {
            int port = 12345; // Port d'écoute
            TcpListener server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Console.WriteLine($"Serveur en écoute sur le port {port}...");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient(); // Accepte une connexion
                Console.WriteLine("Client connecté !");
                NetworkStream stream = client.GetStream();

                // Lire les données du client
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Reçu: {message}");

                // Répondre au client
                string response = "Message reçu";
                byte[] responseData = Encoding.UTF8.GetBytes(response);
                stream.Write(responseData, 0, responseData.Length);

                // Fermer la connexion
                client.Close();
            }
        }
    }
}
