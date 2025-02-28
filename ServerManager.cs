using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EasySave.Models;
using Logger;

namespace EasySave
{
    public class ServerManager
    {
        public ModelConnection ModelConnection = new ModelConnection();

        private static BackupManager BackupManager => BackupManager.GetInstance();          // Backup manager instance
        private CancellationTokenSource _cancellationTokenSource;

        public event EventHandler<Socket> ConnectionAccepted;
        public event EventHandler<string> ConnectionStatusChanged;
        private Socket ServerSocket;
        private static readonly string ConfigFilePath = "Config\\config.json";                                          // Config file path
        private static ServerManager? ServerManager_Instance;
        private string _command;

        // Liste des clients connectés
        private List<Socket> _connectedClients = new List<Socket>();

        private ServerManager()
        {
        }

        public static ServerManager GetInstance()
        {
            ServerManager_Instance ??= new ServerManager();     // Create backup manager instance if not exists
            return ServerManager_Instance;                      // Return backup manager instance
        }

        public Socket StartSocketServer()
        {
            try
            {
                ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(ModelConnection.ConnectionIp, ModelConnection.ConnectionPort);
                ServerSocket.Bind(endPoint);
                ServerSocket.Listen(5);
                ModelConnection.ServerStatus = "Server ACTIVE";
                _cancellationTokenSource = new CancellationTokenSource();
                return ServerSocket;
            }
            catch (SocketException)
            {
                ModelConnection.ServerStatus = "Server ERROR";
                throw;
            }
        }

        public async Task AcceptConnectionAsync(Socket serverSocket)
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    ModelConnection.Client = await Task.Run(() => serverSocket.Accept(), _cancellationTokenSource.Token);
                    _connectedClients.Add(ModelConnection.Client); // Ajouter le client à la liste
                    ConnectionAccepted?.Invoke(this, ModelConnection.Client);
                    ModelConnection.ConnectionStatus = "Connected";
                    ConnectionStatusChanged?.Invoke(this, "Connected");
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

        public async Task ListenToClient()
        {
            byte[] buffer = new byte[4096];
            try
            {
                while (ModelConnection.Client.Connected)
                {
                    int receivedBytes = ModelConnection.Client.Receive(buffer);
                    if (receivedBytes == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                    Console.WriteLine("Client: " + message);

                    string response = await HandleClientCommandAsync(message);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    ModelConnection.Client.Send(responseBytes);
                }
                StopSocketServer(ModelConnection.Client);
            }
            catch (SocketException)
            {
                Console.WriteLine("Connexion perdue avec le client.");
                ModelConnection.ConnectionStatus = "Disconnected";
                ConnectionStatusChanged?.Invoke(this, "Disconnected");
            }
        }

        private async Task<string> HandleClientCommandAsync(string command)
        {
            var commandParts = command.Split(' ');
            var mainCommand = commandParts[0].ToUpper();
            var jobName = commandParts.Length > 1 ? commandParts[1] : string.Empty;

            switch (mainCommand)
            {
                case "EXECUTE_BACKUP":
                    return StartBackup(jobName);
                case "STOP_BACKUP":
                    return StopBackup();
                case "GET_CONFIG":
                    return await SendConfigFileAsync(ModelConnection.Client);
                case "STATUS":
                    return BackupManager.JsonState.Any(s => s.State == "ACTIVE") ? "Backup is running." : "Backup is not running.";
                default:
                    return "Unknown command.";
            }
        }

        public async Task<string> SendConfigFileAsync(Socket client)
        {
            try
            {
                ModelConfig configContent = JsonManager.LoadJson(ConfigFilePath, new ModelConfig()); // Charger le fichier de configuration
                string jsonConfig = JsonSerializer.Serialize(configContent); // Convertir en chaîne JSON
                //SendMessageToAllClients(jsonConfig); // Envoyer la chaîne JSON à tous les clients connectés
                await SendCommandAsync(client, jsonConfig); // Envoyer la chaîne JSON au client
                return "";
            }
            catch (Exception ex)
            {
                return $"Error sending config file: {ex.Message}";
            }
        }

        public async Task SendCommandAsync(Socket socket, string command)
        {
            if (socket.Connected)
            {
                _command = command;
                socket.Send(Encoding.UTF8.GetBytes(command));
            }
            
        }

        private string StartBackup(string jobName)
        {
            ModelJob? modelJob = BackupManager.JsonConfig.BackupJobs.FirstOrDefault(j => j.Name.Equals(jobName, StringComparison.OrdinalIgnoreCase));
            if (modelJob == null)
            {
                return $"No backup job found with name {jobName}.";
            }

            if (modelJob.State.State == "ACTIVE")
            {
                return "A backup is already running.";
            }

            Task.Run(async () => await BackupManager.ExecuteBackupJobAsync(modelJob));
            return $"Backup job '{jobName}' started.";
        }

        private string StopBackup()
        {
            var activeJob = BackupManager.JsonState.FirstOrDefault(s => s.State == "ACTIVE");
            if (activeJob == null)
            {
                return "No active backup to stop.";
            }

            // Logic to stop the backup process
            activeJob.State = "STOPPED";
            return "Backup stopped.";
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
                _connectedClients.Remove(socket); // Retirer le client de la liste
            }
            ModelConnection.ServerStatus = "Server INACTIVE";
            ModelConnection.ConnectionStatus = "Disconnected";
            ConnectionStatusChanged?.Invoke(this, "Disconnected");
        }

        // Méthode pour envoyer des messages à tous les clients connectés
        public void SendMessageToAllClients(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            foreach (var client in _connectedClients)
            {
                if (client.Connected)
                {
                    client.Send(messageBytes);
                }
            }
        }
    }
}
