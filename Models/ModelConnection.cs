using System.ComponentModel;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace EasySave.Models
{
    /// <summary>
    /// Model to describe the connection settings of the application
    /// </summary>
    public class ModelConnection : INotifyPropertyChanged
    {
        /// <summary>
        /// The server socket
        /// </summary>
        public Socket? Server { get; set; } = null;

        /// <summary>
        /// The client socket
        /// </summary>
        public Socket? Client { get; set; } = null;

        /// <summary>
        /// The status of the server
        /// </summary>
        private bool _serverStatus = false;
        public bool ServerStatus
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
        private bool _clientStatus = false;
        public bool ClientStatus
        {
            get { return _clientStatus ; }
            set
            {
                if (_clientStatus != value)
                {
                    _clientStatus = value;
                    OnPropertyChanged(nameof(ClientStatus));
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
