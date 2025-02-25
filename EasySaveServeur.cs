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
        private CancellationTokenSource _cancellationTokenSource;

        public event EventHandler<Socket> ConnectionAccepted;

        public Socket StartSocketServer(int port)
        {
            try
            {
                Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(ModelConnection.ConnectionIp, ModelConnection.ConnectionPort);
                serverSocket.Bind(endPoint);
                serverSocket.Listen(5);
                ModelConnection.ServerStatus = "Server ACTIVE";
                _cancellationTokenSource = new CancellationTokenSource();
                return serverSocket;
            }
            catch (SocketException)
            {
                throw new SocketException();
            }
        }

        public async Task AcceptConnectionAsync(Socket serverSocket)
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    Socket clientSocket = await Task.Run(() => serverSocket.Accept(), _cancellationTokenSource.Token);
                    ConnectionAccepted?.Invoke(this, clientSocket);
                }
                catch (OperationCanceledException)
                {
                    // L'acceptation a été annulée
                    break;
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
                {
                    // Handle the specific case where the accept call was interrupted
                    break;
                }
            }
        }


        public async Task ListenToClient(Socket client)
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (client.Connected)
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
                _cancellationTokenSource?.Cancel();
                if (socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                socket.Close();
                ModelConnection.ServerStatus = "Server INACTIVE";
            }
        }
    }
}
