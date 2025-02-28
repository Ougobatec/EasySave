using System.ComponentModel;
using System.Net;

namespace EasySave.Models
{
    /// <summary>
    /// Model to describe the connection settings of the application
    /// </summary>
    public class ModelConnection : INotifyPropertyChanged
    {
        /// <summary>
        /// The name of the connection
        /// </summary>
        public string ConnectionName { get; set; } = "EasySaveServer";

        /// <summary>
        /// The IP address of the connection
        /// </summary>
        public IPAddress ConnectionIp { get; set; } =  IPAddress.Any;

        /// <summary>
        /// The port of the connection
        /// </summary>
        public int ConnectionPort { get; set; } = 12345;

        /// <summary>
        /// The status of the server
        /// </summary>
        private string _serverStatus = "Server INACTIVE";
        public string ServerStatus
        {
            get { return _serverStatus; }
            set
            {
                if (_serverStatus != value)
                {
                    _serverStatus = value;
                    OnPropertyChanged(nameof(ServerStatus));
                }
            }
        }

        /// <summary>
        /// The status of the connection
        /// </summary>
        private string _connectionStatus = "Disconnected";
        public string ConnectionStatus
        {
            get { return _connectionStatus ; }
            set
            {
                if (_connectionStatus != value)
                {
                    _connectionStatus = value;
                    OnPropertyChanged(nameof(ConnectionStatus));
                }
            }
        }

        /// <summary>
        /// The event to notify the change of a property
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notify the change of a property
        /// <param name="propertyName">the name of the property</param>
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
