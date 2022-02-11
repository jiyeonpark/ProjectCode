#if SERVER_UNITY

using System;
using System.Net.Sockets;

namespace WCS.Network
{
    class AcceptToken
    {
        private int _id = 0;
        private int _socket_handle_number = 0;

        public AcceptToken(int id)
        {
            _id = id;
        }
        
        public int Id
        {
            get { return _id; }
        }

        public int SocketHandleNumber
        {
            get { return _socket_handle_number; }
        }
    }
}

#endif