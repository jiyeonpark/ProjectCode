using System;
using System.Collections.Concurrent;
using System.Threading;
using WCS;
namespace WCS.Network
{
    public class ClientNetwork
    {
        public enum SessionEventType
        {
            Connected,             // All transports connected
            
            DisConnected,
            Opened,                // Session opened            
            Stopped,               // Session stopped
            Closed,                // Session closed
            RedirectStarted,       // Server move started
            RedirectSucceeded,     // Server move successful
            RedirectFailed,         // Server move failed
            ConnectedFailed             // All transports connected

        };

        private ClientTokenManager _clientTokenManager = null;
        private SendBufferPool _sendBufferPool = null;
        private Connector _connector = null;
        private netContext _netContext = null;
      
        private ConnectToken _connectToken = null;
        public ManualResetEvent PacketEvent { get; private set; }
        //public GamePacketProcess _PacketProcess { get; private set; }

        public event Action<SessionEventType, ClientToken> SessionEventCallback = null;
#if !SERVER_UNITY
        public event Action<ClientToken, bool> SessionOpenCallback = null;
        public event Action<long, wce_err> SessionCloseCallback = null;
#endif


        public void Initialize(string serverName, int max_session, int net_context_default_size, int net_context_create_size)
        {

            Network.SessionPool.instance.Initialize(max_session, 1);
            _clientTokenManager = new ClientTokenManager();
            _sendBufferPool = new SendBufferPool();
            _connector = new Connector(serverName, _clientTokenManager);
                      
            _netContext = new netContext(net_context_default_size, net_context_create_size);
            PacketEvent = new ManualResetEvent(false);

            _sendBufferPool.Initialize(max_session);

#if SERVER_UNITY
            _clientTokenManager.Initialize(serverName, max_session, _sendBufferPool, true);
#else
            _clientTokenManager.Initialize(serverName, max_session, _sendBufferPool, false);
#endif
            _clientTokenManager.CreateEventArgs(this.SetGamePacketProcess);
                        
            _clientTokenManager.ConnectedCallback += this.Connected;
            _clientTokenManager.DisConnectedCallback += this.DisConnected;
            
            _clientTokenManager.ConnecFailCallback += this.ConnectFail;


#if !SERVER_UNITY
            _clientTokenManager.SessionOpenCallback += this.OnSessionOpen;
            _clientTokenManager.SessionCloseCallback += this.OnSessionClose;
#endif
        }

        public bool Connect(string ip, int port)
        {
            if (null != _connectToken)
            {                
                _connectToken = null;
            }

            _connectToken = new ConnectToken(ip, port);

            if (false == _connector.PostConnect(_connectToken))
            {
                _connectToken = null;
            }

            return true;
        }

        public bool ReConnect(string ip, int port)
        {
            if (null == _connectToken)
            {
                Connect(ip, port);
                return false;
            }

            if (false == _connector.PostConnect(_connectToken))
            {
                _connectToken = null;
                return false;
            }

            return true;
        }

        public void DisconnetNetwork()
        {
            if (null == _connectToken || null == _connectToken.clientToken)
                return;

            _connectToken.clientToken.CloseNetwork(); 
            _connectToken.clientToken = null;

        }
        
        public void Connected(ConnectToken token)
        {
            if (null != SessionEventCallback)
            {
                SessionEventCallback(SessionEventType.Connected, token.clientToken);
            }
        }
        public void ConnectFail(ConnectToken token)
        {
            if (null != SessionEventCallback)
            {
                SessionEventCallback(SessionEventType.ConnectedFailed, token.clientToken);
            }
        }
        public void DisConnected(ConnectToken token)
        {
            PacketEvent.Set();

            if (null != SessionEventCallback)
            {
                SessionEventCallback(SessionEventType.DisConnected, token.clientToken);
            }
        }

#if !SERVER_UNITY
        public void OnSessionOpen(Session session, bool reconnect)
        {
            if (null != SessionOpenCallback)
            {
                SessionOpenCallback(session.clientToken, reconnect);
            }
        }

        public void OnSessionClose(long SessionID, wce_err err)
        {
            if (null != SessionCloseCallback)
            {
                SessionCloseCallback(SessionID, err);
            }
        }
#endif

        public void SetGamePacketProcess(ClientToken token, ReadStream stream)
        {
            _netContext.PushQueue(token, stream);
            PacketEvent.Set();
        }

        public void ContextSwitchIndex()
        {
            _netContext.SwitchIndex();
        }

        public bool GetContext(out netContext.Context context)
        {
            return _netContext.PopQueue(out context);
        }

        public void PushContext(netContext.Context context)
        {
            ReadStreamPool.instance.Push(context.stream);

            if (null == _netContext)
                return;

            _netContext.Push(context);
        }
      
        public bool SendMessage<T>(T packet) where T : class, ISerialize
        {
            if (null != _connectToken && true == _connectToken.IsConnected())
            {
                _connectToken.clientToken.Send(packet);
                return true;
            }

            return false;
        }

        public ConnectToken GetConnectToken()
        {
            return _connectToken;
        }
    }
}