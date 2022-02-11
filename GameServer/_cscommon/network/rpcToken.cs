#if SERVER_UNITY

namespace WCS.Network
{
    public class RpcToken : ConnectToken
    {
        private readonly wre_server_type _server_type;
        private readonly int _index;
        private readonly string _ip;
        private readonly int _port;

        public wre_server_type serverType
        {
            get { return _server_type; }
        }

        public int index
        {
            get { return _index; }
        }

        public string ip
        {
            get { return _ip; }
        }

        public int port
        {
            get { return _port; }
        }

        public RpcToken(wre_server_type server_type, int index, string ip, int port) : base (ip, port)
        {
            _server_type = server_type;
            _index = index;
            _ip = ip;
            _port = port;
        }
    }
}

#endif