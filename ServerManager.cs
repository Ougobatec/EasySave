using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using EasySave.Models;

namespace EasySave
{
    /// <summary>
    /// Server manager class to handle the server connection
    /// </summary>
    public class ServerManager
    {
        public ModelConnection Connection = new ModelConnection();                          // Connection model instance
        private static BackupManager BackupManager => BackupManager.GetInstance();          // Backup manager instance
        private static ServerManager? ServerManager_Instance;                               // Server manager instance
        private CancellationTokenSource? cancellationTokenSource;                           // Cancellation token source

        /// <summary>
        /// Get the ServerManager instance or create it if it doesn't exist
        /// </summary>
        public static ServerManager GetInstance()
        {
            ServerManager_Instance ??= new ServerManager();     // Create backup manager instance if not exists
            return ServerManager_Instance;                      // Return backup manager instance
        }

        /// <summary>
        /// Start the server
        /// </summary>
        public async Task StartServer()
        {
            try
            {
                cancellationTokenSource = new CancellationTokenSource();                                            // Create a token source
                Connection.ServerStatus = true;                                                                     // Set the server status to true
                Connection.Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);    // Create a new server socket
                Connection.Server.Bind(new IPEndPoint(IPAddress.Any, 5000));                                        // Bind the server socket to the IP address and port
                Connection.Server.Listen(1);                                                                        // Listen for incoming connections

                Connection.Client = await Task.Run(() => Connection.Server.Accept());                               // Accept the client connection
                Connection.ClientStatus = true;                                                                     // Set the client status to true

                SendConfigAsync(Connection.Client);                                                                 // Send the configuration file to the client

                await HandleClientAsync(Connection.Client, cancellationTokenSource.Token);                          // Handle the client connection
            }
            finally
            {
                DisconnectClient(Connection.Client);                                                                // Set the client status to false
            }
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public void StopServer()
        {
            Task.Run(() =>
            {
                try
                {
                    cancellationTokenSource?.Cancel();                          // Cancel the token source

                    if (Connection.Client != null)
                    {
                        DisconnectClient(Connection.Client);                    // Disconnect the client
                    }

                    if (Connection.Server != null)
                    {
                        Connection.Server.Close();                              // Close the server socket
                        Connection.Server = null;                               // Set the server socket to null
                    }

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Connection.ServerStatus = false;                        // Set the server status to false
                        Connection.ClientStatus = false;                        // Set the client status to false
                    });
                }
                catch (Exception ex)
                {
                    // Do nothing
                }
            });
        }

        /// <summary>
        /// Send the configuration file to the client
        /// </summary>
        public async void SendConfigAsync(Socket socket)
        {
            if (Connection.ClientStatus)
            {
                ModelConfig configContent = BackupManager.JsonConfig;                   // Load the configuration file
                string jsonConfig = JsonSerializer.Serialize(configContent);            // Convert to JSON string
                byte[] data = Encoding.UTF8.GetBytes(jsonConfig);                       // Convert to byte array

                await socket.SendAsync(data, SocketFlags.None);                         // Send the byte array to the client
            }
        }

        /// <summary>
        /// Handle the client connection
        /// <param name="socket">The client socket</param>
        /// </summary>
        private async Task HandleClientAsync(Socket socket, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[1024];                                                                         // Create a buffer to store the data

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    int receivedBytes = await socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);     // Receive the data from the client
                    if (receivedBytes == 0) break;                                                                  // Break the loop if no data is received

                    string message = Encoding.UTF8.GetString(buffer, 0, receivedBytes);                             // Convert the byte array to a string

                    string[] parts = message.Split(' ', 2);                                                         // Split the message into parts
                    if (parts.Length < 2)
                    {
                        continue;                                                                                   // Continue the loop if the message is invalid
                    }

                    string command = parts[0];                                                                      // Get the command
                    string backupName = parts[1];                                                                   // Get the value

                    switch (command.ToUpper())
                    {
                        case "EXECUTE":
                            ExecuteBackup(backupName);                                                              // Execute the backup
                            break;
                        case "PAUSE":
                            PauseBackup(backupName);                                                                // Pause the backup
                            break;
                        case "STOP":
                            StopBackup(backupName);                                                                 // Stop the backup
                            break;
                        case "EXIT":
                            DisconnectClient(socket);                                                               // Disconnect the client
                            break;
                        default:
                            break;
                    }
                }
            }
            finally
            {
                DisconnectClient(socket);                                                                           // Disconnect the client
            }
        }

        /// <summary>
        /// Disconnect the client
        /// <param name="socket">The client socket</param>
        /// </summary>
        private void DisconnectClient(Socket socket)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);   // Shutdown the client socket
                socket.Close();                         // Close the client socket

                Connection.ClientStatus = false;        // Set the client status to false
            }
            catch (Exception)
            {
                // Do nothing
            }
        }

        /// <summary>
        /// Execute the backup
        /// <param name="backupName">The name of the backup</param>
        /// </summary>
        private void ExecuteBackup(string backupName)
        {
            ModelJob? job = BackupManager.JsonConfig.BackupJobs.FirstOrDefault(j => j.Name.Equals(backupName, StringComparison.OrdinalIgnoreCase));
            if (job == null)
            {
                return;
            }
            Task.Run(async () => await BackupManager.ExecuteBackupJobAsync(job));
        }

        /// <summary>
        /// Pause the backup
        /// <param name="backupName">The name of the backup</param>
        /// </summary>
        private void PauseBackup(string backupName)
        {
            ModelJob? job = BackupManager.JsonConfig.BackupJobs.FirstOrDefault(j => j.Name.Equals(backupName, StringComparison.OrdinalIgnoreCase));
            if (job == null)
            {
                return;
            }
            Task.Run(async () => await BackupManager.PauseBackupJobAsync(job));
        }

        /// <summary>
        /// Stop the backup
        /// <param name="backupName">The name of the backup</param>
        /// </summary>
        private void StopBackup(string backupName)
        {
            ModelJob? job = BackupManager.JsonConfig.BackupJobs.FirstOrDefault(j => j.Name.Equals(backupName, StringComparison.OrdinalIgnoreCase));
            if (job == null)
            {
                return;
            }
            Task.Run(async () => await BackupManager.StopBackupJobAsync(job));
        }
    }
}
