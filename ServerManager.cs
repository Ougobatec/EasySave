﻿using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasySave.Enumerations;
using EasySave.Models;

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

        private string _command;

        // Liste des clients connectés
        private readonly List<Socket> _connectedClients = new List<Socket>();

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
                    Socket clientSocket = await Task.Run(() => serverSocket.Accept(), _cancellationTokenSource.Token);
                    _connectedClients.Add(clientSocket); // Ajouter le client à la liste
                    ConnectionAccepted?.Invoke(this, clientSocket);
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

                    string response = await HandleClientCommandAsync(message);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    client.Send(responseBytes);
                }
                StopSocketServer(client);
            }
            catch (SocketException)
            {
                Console.WriteLine("Connexion perdue avec le client.");
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
                    return await SendConfigFileAsync();
                case "STATUS":
                    return BackupManager.JsonState.Any(s => s.State == BackupStates.ACTIVE) ? "Backup is running." : "Backup is not running.";
                default:
                    return "Unknown command.";
            }
        }

        private async Task<string> SendConfigFileAsync()
        {
            try
            {
                string configContent = await File.ReadAllTextAsync(ConfigFilePath);
                SendMessageToAllClients(configContent);
                return "Config file sent.";
            }
            catch (Exception ex)
            {
                return $"Error sending config file: {ex.Message}";
            }
        }
       

        public async Task SendCommandAsync(Socket socket, string command)
        {
            _command = command;
            socket.Send(Encoding.UTF8.GetBytes(command));
        }

        private string StartBackup(string jobName)
        {
            ModelJob? modelJob = BackupManager.JsonConfig.BackupJobs.FirstOrDefault(j => j.Name.Equals(jobName, StringComparison.OrdinalIgnoreCase));
            if (modelJob == null)
            {
                return $"No backup job found with name {jobName}.";
            }

            if (modelJob.State.State == BackupStates.ACTIVE)
            {
                return "A backup is already running.";
            }

            Task.Run(async () => await BackupManager.ExecuteBackupJobAsync(modelJob));
            return $"Backup job '{jobName}' started.";
        }

        private string StopBackup()
        {
            var activeJob = BackupManager.JsonState.FirstOrDefault(s => s.State == BackupStates.ACTIVE);
            if (activeJob == null)
            {
                return "No active backup to stop.";
            }

            // Logic to stop the backup process
            activeJob.State = BackupStates.READY;
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
