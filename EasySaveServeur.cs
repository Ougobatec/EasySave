using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasySave.Models;

namespace EasySave
{
    public class EasySaveServer 
    {
        public ModelConnection ModelConnection = new ModelConnection();
       
        /// <summary>
        /// Start the socket server
        /// </summary>
        public Socket StartSocketServer(int port)
        {
            try
            {
                // Création du socket serveur
                Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Définition de l'adresse IP et du port
                IPEndPoint endPoint = new IPEndPoint(ModelConnection.ConnectionIp, ModelConnection.ConnectionPort);

                // Association du socket à l'adresse et au port
                serverSocket.Bind(endPoint);

                // Mise à l'écoute du socket
                serverSocket.Listen(5);
                
                ModelConnection.ServerStatus = "Server ACTIVE";
                return serverSocket;
            }
            catch (SocketException)
            {
                throw new SocketException();
            }

        }

        //public Socket AcceptConnection(Socket serverSocket)
        //{
        //    Socket SocketClient = serverSocket.Accept();
        //    IPEndPoint clientEndPoint = (IPEndPoint)SocketClient.RemoteEndPoint;
        //    ModelConnection.ConnectionStatus = "Connected";
        //    return SocketClient;
        //}

        public async Task<Socket> AcceptConnectionAsync(Socket serverSocket)
        {
            return await Task.Run(() => serverSocket.Accept());
        }

        public async Task ListenToClient(Socket client)
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (client.Connected) // Vérifie que le client est bien connecté
                {
                    
                    int receivedBytes = client.Receive(buffer);
                    if (receivedBytes == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, receivedBytes);

                    Console.WriteLine("Client: " + message);


                    string response = message.ToUpper();

                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    client.Send(responseBytes);


                }
                ModelConnection.ConnectionStatus = "Not Connected";
            }
            catch (SocketException)
            {
                Console.WriteLine("Connexion perdue avec le client.");
            }

        }

        public void StopSocketServer(Socket socket)
        {
            if (socket != null)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                
            }
        }

        
    }
}
