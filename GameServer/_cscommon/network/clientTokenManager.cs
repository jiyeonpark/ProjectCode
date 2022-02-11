using System;
using System.Net.Sockets;
using System.Threading;

namespace WCS.Network
{
    public class ClientTokenManager
    {
        private SocketAsyncEventArgsPairPool _event_args_pair_pool = null;
        private SocketAsyncEventBufferManager _buffer_manager = null;
        private SendBufferPool _sendBufferPool = null;
        private int _max_session = 0;
        private string _serverName = string.Empty;
        private bool _isRpc = false;
        public event Action<ClientToken> AcceptedCallback = null;
        public event Action<ClientToken> DisConnectedAcceptCallback = null;
        public event Action<ConnectToken> ConnectedCallback = null;
        public event Action<ConnectToken> ConnecFailCallback = null;
        public event Action<ConnectToken> DisConnectedCallback = null;

        public event Action<Session, bool> SessionOpenCallback = null;
        public event Action<long, wce_err> SessionCloseCallback = null;

        private Int32 numConnectedSockets;

        

        //internal Int32 numberOfAcceptedSockets;
        //private Semaphore semaphoreAcceptedClients;
        public void Initialize(string serverName, int max_session, SendBufferPool sendBufferPool, bool bRpc)
        {
            _serverName = serverName;
            _max_session = max_session;
            _event_args_pair_pool = new SocketAsyncEventArgsPairPool();
            _buffer_manager = new SocketAsyncEventBufferManager(_max_session * NET_define.ASYNC_EVENT_BUFFER_SIZE * 2, NET_define.ASYNC_EVENT_BUFFER_SIZE);
            _sendBufferPool = sendBufferPool;
            _isRpc = bRpc;
            _buffer_manager.InitBuffer();
            numConnectedSockets = 0;
            //this.numberOfAcceptedSockets = 0;
            
        }
#if SERVER_UNITY
        public void CreateEventArgs_rpc(rpcPacketProcess packetProcess)
        {
            //this.semaphoreAcceptedClients = new Semaphore(_max_session, _max_session);
            for (int i = 0; i < _max_session; i++)
            {
                var temp = new SocketAsyncEventArgsPair();

                var token = new ClientToken(_serverName, this, _sendBufferPool, temp, _isRpc);
                
                {
                    temp.RecvEventArgs = new SocketAsyncEventArgs();
                    temp.RecvEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(this.eventHander_Recv);
                    temp.RecvEventArgs.UserToken = token;

                    _buffer_manager.SetBuffer(temp.RecvEventArgs);
                    
                }

                {
                    temp.SendEventArgs = new SocketAsyncEventArgs();
                    temp.SendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(this.eventHander_Send);
                    temp.SendEventArgs.UserToken = token;

                    _buffer_manager.SetBuffer(temp.SendEventArgs);
                }

                token._rpcPacketProcess += packetProcess;

                _event_args_pair_pool.Push(temp);
            }
        }
#endif
        public void CreateEventArgs(PacketProcess packetProcess)
        {
            //this.semaphoreAcceptedClients = new Semaphore(_max_session, _max_session);
            for (int i = 0; i < _max_session; i++)
            {
                var temp = new SocketAsyncEventArgsPair();

                var token = new ClientToken(_serverName, this, _sendBufferPool, temp, _isRpc);
                token.TokenId = _event_args_pair_pool.AssignTokenId() + 1000000;
                {
                    temp.RecvEventArgs = new SocketAsyncEventArgs();
                    temp.RecvEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(this.eventHander_Recv);
                    temp.RecvEventArgs.UserToken = token;

                    _buffer_manager.SetBuffer(temp.RecvEventArgs);
                }

                {
                    temp.SendEventArgs = new SocketAsyncEventArgs();
                    temp.SendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(this.eventHander_Send);
                    temp.SendEventArgs.UserToken = token;

                    _buffer_manager.SetBuffer(temp.SendEventArgs);
                }

                token._packetProcess += packetProcess;

                _event_args_pair_pool.Push(temp);
            }
        }

        public int PoolSize()
        {
            return _event_args_pair_pool.Count;
        }

        public void eventHander_Connected(Socket acceptSocket /* acceptor 일 경우*/, ConnectToken connectToken /* connector 일 경우*/)
        {
            var eventArgs = _event_args_pair_pool.Pop();

            WCS.logger.Info($"eventHander_Connected({_serverName}). _event_args_pair_pool size : {PoolSize()}");

            if (null == eventArgs)
            {
                WCS.logger.Error($"eventHander_Connected({_serverName}). _event_args_pair_pool is empty. eventArgs is null.");
                return;
            }
            
            var token = eventArgs.RecvEventArgs.UserToken as ClientToken;
            
            Int32 numberOfConnectedSockets = Interlocked.Increment(ref this.numConnectedSockets);
            if (null != acceptSocket)
            {
                token.socket = acceptSocket;
                
                
                WCS.logger.Info($"Accept Socket Connected [{_serverName}] - " + acceptSocket.RemoteEndPoint.ToString()+ $"{ numConnectedSockets}");
            }
            else if (null != connectToken)
            {
                token.socket = connectToken.socket;
                token.IsConnecter = true;
                WCS.logger.Info($"Connector Socket Connected [{_serverName}] - " + connectToken.socket.RemoteEndPoint.ToString());
            }
            else
            {
                WCS.logger.Error("socket object is null.");
                return;
            }
            token.IsConnected = true;
            token.SetSocketOption(token.socket);

            token.PostReceive();
            // rpc  or  client
            if (null != connectToken)   
            {
                // server에 연결되고 session 인증 보냄
                {
                    var message = new WCS.Network.wcg_session_certify_cs();
                    if (token == null)
                        message.session_id = 0;
                    else
                        message.session_id = token.SessionID;

                    logger.Debug($"token.Send : {message.session_id} ");
                    token.Send(message);

                    
                }

                connectToken.clientToken = token;
                token.connectToken = connectToken;

                if (null != this.ConnectedCallback)
                {
                    ConnectedCallback(token.connectToken);
                }
            }   
            else if (null != acceptSocket)
            {
                if (null != this.AcceptedCallback)
                {
                    AcceptedCallback(token);
                }
            }

            

        }

        public void eventHander_ConnectFail(ConnectToken connectToken)
        {
            if (null != this.ConnecFailCallback)
            {
                ConnecFailCallback(connectToken);
            }
        }

        public void eventHander_Recv(object sender, SocketAsyncEventArgs e)
        {
            if (SocketAsyncOperation.Receive == e.LastOperation)
            {
                var token = e.UserToken as ClientToken;
                token.ReceiveProcess();
            }
            else
            {
                throw new ArgumentException("The last operation completed on the socket was not a receive.");
            }
        }

        public void eventHander_Send(object sender, SocketAsyncEventArgs e)
        {
            if (SocketAsyncOperation.Send == e.LastOperation)
            {
                var token = e.UserToken as ClientToken;
                token.SendProcess(e);
            }
            else
            {
                throw new ArgumentException("The last operation completed on the socket was not a receive.");
            }
        }
        public void SessionOpenClient(Session session, bool bReconnect)
        {
            if (null != this.SessionOpenCallback)
            {
                SessionOpenCallback(session, bReconnect);
                
            }
        }


        public void SessionCloseClient(long SessionID, wce_err err)
        {
            try
            {
                if (null != this.SessionCloseCallback)
                {
                    
                    SessionCloseCallback(SessionID, err);

                    WCS.logger.Warn($"SessionCloseClient({_serverName}). number :{numConnectedSockets}");
                }
            }
            catch (Exception ex)
            {
                WCS.logger.Error(ex.Message);
            }


        }
        public void CloseClientSocket(ClientToken token)
        {
            if (null != token.connectToken)
            {
                if (null != this.DisConnectedCallback)
                {
                    this.DisConnectedCallback(token.connectToken);
                }
            }
            else if (null != this.DisConnectedAcceptCallback)
            {
                this.DisConnectedAcceptCallback(token);

            }

            if (null != token && token.socket != null)
            {

                try
                {

                    token.socket.Shutdown(SocketShutdown.Both);
                }
                // throws if socket was already closed
                catch (Exception)
                {

                    WCS.logger.Info("Shutdown catch, id " + token.TokenId + "\r\n");

                }
                
                if (token.socket != null)
                    token.socket.Close();
                token.Reset();
                // Decrement the counter keeping track of the total number of clients connected to the server.
                //this.semaphoreAcceptedClients.Release();
                Interlocked.Decrement(ref this.numConnectedSockets);
                

                _event_args_pair_pool.Push(token.EventArgsPair);
                WCS.logger.Warn($"CloseClientSocket({_serverName}). _event_args_pair_pool size : {PoolSize()} number :{numConnectedSockets}");
                
                
                
            }
  
        }
    }
}