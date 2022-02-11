using System;
using System.Net;
using System.Net.Sockets;

namespace WCS.Network
{
    public class ConnectToken
    {
        protected readonly Socket _socket = null;
        protected readonly IPEndPoint _endpoint = null;
        protected ClientToken _clientToken = null;
        protected bool _connected = false;

        public Socket socket
        {
            get { return _socket; }
        }

        public IPEndPoint endpoint
        {
            get { return _endpoint; }
        }

        public ClientToken clientToken
        {
            get { return _clientToken; }
            set { _clientToken = value; }
        }

        public bool IsConnected()
        {
            if (null != _clientToken && null != _clientToken.socket)
            {
                return _clientToken.socket.Connected;
            }
            return false;
        }    
        
        public ConnectToken(string ip, int port)
        {
            _endpoint = new IPEndPoint(Dns.GetHostAddresses(ip)[0], port);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);            
        }
    }
}