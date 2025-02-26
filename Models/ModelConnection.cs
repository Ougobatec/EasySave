using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Models
{
    public class ModelConnection : INotifyPropertyChanged
    {
        
        public string ConnectionName { get; set; } = "EasySaveServer";
        public IPAddress ConnectionIp { get; set; } =  IPAddress.Any;
        public int ConnectionPort { get; set; } = 12345;

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
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
