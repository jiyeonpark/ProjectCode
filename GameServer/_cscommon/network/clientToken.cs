
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using WCS.Util;

namespace WCS.Network
{
    public delegate bool PacketHookHandler(WCS.Network.ClientToken token, ReadStream stream);
#if SERVER_UNITY
    public delegate bool rpcPacketHookHandler(WCS.Network.ClientToken token, ReadStream stream);
#endif

    public delegate void PacketProcess(WCS.Network.ClientToken token, ReadStream stream);
#if SERVER_UNITY
    public delegate void rpcPacketProcess(WCS.Network.ClientToken token, ReadStream stream);
#endif
   
    public delegate void OnSessionOpened(WCS.Network.Session session, bool reconnect = false);
    public delegate void OnSessionClosed(WCS.Network.Session session);

    public delegate void OnSocketOpened(WCS.Network.ClientToken socket, bool reconnect = false);
    public delegate void OnSocketClosed(WCS.Network.ClientToken socket);
    public delegate void OnNetworkFault(WCS.Network.Session session);

#if SERVER_UNITY
    public delegate void ConnectedCallback(WCS.Network.wre_server_type server_type, int server_index, WCS.Network.ClientToken token);
#endif
    public delegate void ConnecFailCallback(WCS.Network.ClientToken token);


    public class ClientToken : IDisposable
    {
        private Int32 idToken = 0;
        private ClientTokenManager  _clientTokenManager = null;
        private SendBufferPool      _sendBufferPool = null;
        private Socket              _socket = null;
        private SocketAsyncEventArgsPair _event_args = null;
        private ReadBuffer          _read_buffer = null;
        private SendBuffer          _send_buffer = null;
        private int                 _send_use_count = 0;
        private int                 _send_request_size = 0;        
        public ConnectToken         _connectToken = null;
        private long                _connector_session_id = 0;
        private long                _rpc_session_id = 0;

        private string              _serverName = string.Empty;
        
        
        private long                _recv_count = 0;
        private int                 _next_time = 0;
        private bool                _is_rpc = false;
        private bool                _connected = false;
        private bool                _connecter = false;

        internal int                _disconnected = 0;

        private long                expire_time = 0;
        public PacketProcess _packetProcess { get; set; }
#if SERVER_UNITY
        public rpcPacketProcess _rpcPacketProcess { get; set; }
#endif
        public long SessionID
        {
            get { return _connector_session_id; }
            set { _connector_session_id = value; }
        }
        public long RpcSessionID
        {
            get { return _rpc_session_id; }
            set { _rpc_session_id = value; }
        }
        public Int32 TokenId
        {
            get
            {
                return this.idToken;
            }
            set { idToken = value; }
        }
        public Socket socket
        {
            get { return _socket; }
            set { _socket = value; }
        }

        public ConnectToken connectToken 
        {
            get { return _connectToken; }
            set { _connectToken = value; }
        }

        public SocketAsyncEventArgs RecvEventArgs
        {
            get { return _event_args.RecvEventArgs; }
        }

        public SocketAsyncEventArgs SendEventArgs
        {
            get { return _event_args.SendEventArgs; }
        }

        public SocketAsyncEventArgsPair EventArgsPair
        {
            get { return _event_args; }
        }

        public bool IsRpc
        {
            get { return _is_rpc; }
            set { _is_rpc = value; }
        }

        public bool IsConnected
        {
            get { return _connected; }
            set { _connected = value; }
        }

        public long _expire_time
        {
            get { return expire_time; }
            set { expire_time = value; }
        }
        public bool IsConnecter
        {
            get { return _connecter; }
            set { _connecter = value; }
        }

        public ClientToken(string serverName, ClientTokenManager clientTokenManager_, SendBufferPool sendBufferPool_, SocketAsyncEventArgsPair args, bool bRpc )
        {
            _serverName = serverName;
            _clientTokenManager = clientTokenManager_;
            _sendBufferPool = sendBufferPool_;
            _read_buffer = new ReadBuffer();
            _send_buffer = new SendBuffer();
            // _list_stream = new List<ReadStream>();
            _event_args = args;

            _next_time = DateTime.Now.ToUnixTimeInt() + 1;

            IsRpc = bRpc;

            _connected = false;
            _connecter = false;
            
            _expire_time = 0;
            
        }

        
        
        public void CloseNetwork()
        {
            if (0 < Interlocked.Exchange(ref _disconnected, 1))
            {
                //
                WCS.logger.Warn($"{_serverName}, CloseClientSocket CompareExchange => {_disconnected}, {socket?.RemoteEndPoint.ToString()}");
                return;
            }
            _clientTokenManager.CloseClientSocket(this);
            Interlocked.Exchange(ref _disconnected, 0);
        }

        public void Reset()
        {

            //_clientTokenManager.SessionCloseClient(this);
            //_list_stream.Clear();
            _rpc_session_id = 0;
            
            _connected = false;
            _connecter = false;
            _socket = null;
            
            _connectToken = null;
            _read_buffer.Reset();

            _send_buffer.Reset();
            _send_request_size = 0;
            Interlocked.Exchange(ref _send_use_count, 0);
            Interlocked.Exchange(ref _disconnected, 0);
        }
        public void Dispose()
        {
            //try
            //{
            //    this.socket.Shutdown(SocketShutdown.Send);
            //}
            //catch (Exception)
            //{
            //    // Throw if client has closed, so it is not necessary to catch.
            //}
            //finally
            //{
            //    this.socket.Close();
            //}
            WCS.logger.Info($"Dispose");

        }
        public void SetSocketOption(Socket socket)
        {
            try
            {
#if _DUMMY
#else
                // keep alive
                {
                    int size = sizeof(UInt32);
                    UInt32 on = 1;
                    UInt32 keepAliveInterval = 3000;
                    UInt32 retryInterval = 10000;
                    byte[] option = new byte[size * 3];
                    Array.Copy(BitConverter.GetBytes(on), 0, option, 0, size);
                    Array.Copy(BitConverter.GetBytes(keepAliveInterval), 0, option, size, size);
                    Array.Copy(BitConverter.GetBytes(retryInterval), 0, option, size * 2, size);

                    socket.IOControl(IOControlCode.KeepAliveValues, option, null);


                    // socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    // socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 1);
                    // socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1);
                    // socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 5);                    
                }
#endif

                // Disable the Nagle Algorithm for this tcp socket.
                socket.NoDelay = true;
                // Don't allow another socket to bind to this port.
                // socket.ExclusiveAddressUse = true;

                // The socket will linger for 0 seconds after Socket.Close is called.
                socket.LingerState = new LingerOption(enable: true, seconds: 0);
                // Set the receive buffer size
                socket.ReceiveBufferSize = 65536;
                // Set the send buffer size
                socket.SendBufferSize = 65536;
                // Set the timeout for synchronous receive methods to 1 second (1000 milliseconds.)
                socket.ReceiveTimeout = 10000;
                // Set the timeout for synchronous send methods to 1 second (1000 milliseconds.) 
                socket.SendTimeout = 10000;

                
            }
            catch (Exception e)
            {
                WCS.logger.Error(e.Message);
            }
        }

        public void PostReceive()
        {
            try
            {
                if (null != socket)
                {
                    
                    if (false == socket.ReceiveAsync(RecvEventArgs))
                    {
                        ReceiveProcess();
                    }
                }
            }
            catch (Exception e)
            {
                WCS.logger.Error(e.Message);
            }
        }        
        
        public void ReceiveProcess()
        {
#if SERVER_UNITY
            if (IsRpc)
            {
                if (SocketError.Success == RecvEventArgs.SocketError && 0 < RecvEventArgs.BytesTransferred)
                {
                    List<ReadStream> list_stream = null;
                    //_list_stream.Clear();

                    bool complete = _read_buffer.Complete(RecvEventArgs, out list_stream);

                    int current_time = DateTime.Now.ToUnixTimeInt();
                    if (current_time > _next_time)
                    {

                        //WCS.logger.Info($"ReceiveProcess: [{_recv_count}] complete:[{complete}]");
                        _recv_count = 0;
                        _next_time = current_time + 1;
                    }
                                        

                    if (complete && null != list_stream)
                    {
                        if (null != _rpcPacketProcess && 0 < list_stream.Count)
                        {
                            for (int i = 0; i < list_stream.Count; i++)
                            {

                                if ((ushort)WCS.Network.wce_cmd.session_certify_cs == list_stream[i].command)
                                {
                                    if (null == _connectToken) //서버 측에서 받아서 
                                    {
                                        wcg_session_certify_cs packet = new wcg_session_certify_cs();
                                        packet.Deserialize(list_stream[i].br);

                                        SetSession(0);

                                    }

                                }
                                else if ((ushort)WCS.Network.wce_cmd.session_open_sc == list_stream[i].command)
                                {
                                    //RPC 받는부분
                                    {
                                        wcg_session_open_sc packet = new wcg_session_open_sc();
                                        packet.Deserialize(list_stream[i].br);

                                        SetSession(0);
                                    }
                                 
                                }
                                else
                                {
                                    // this.PacketRecvTime = DateTime.Now.Ticks;
                                    _rpcPacketProcess(this, list_stream[i]);
                                }

                                _recv_count++;
                            }

                            
                        }
                    }

                    PostReceive();
                }
                else
                {
                    WCS.logger.Error($"{_serverName}, {RecvEventArgs.SocketError}, transferred {RecvEventArgs.BytesTransferred}");
                    CloseNetwork();
                }
            }
            else
#endif
            {
                if (SocketError.Success == RecvEventArgs.SocketError && 0 < RecvEventArgs.BytesTransferred)
                {
                    List<ReadStream> list_stream = null;
                    

                    bool complete = _read_buffer.Complete(RecvEventArgs, out list_stream);


                    int current_time = DateTime.Now.ToUnixTimeInt();
                    if (current_time > _next_time)
                    {

                        //WCS.logger.Info($"ReceiveProcess: [{_recv_count}] complete:[{complete}]");
                        _recv_count = 0;
                        _next_time = current_time + 1;
                    }

                    if (complete && null != list_stream)
                    {
                        if (null != _packetProcess && 0 < list_stream.Count)
                        {
                            for (int i = 0; i < list_stream.Count; i++)
                            {

                                if ((ushort)WCS.Network.wce_cmd.session_certify_cs == list_stream[i].command)
                                {
                                    if (null == _connectToken) //서버 측에서 받아서 
                                    {
                                        wcg_session_certify_cs packet = new wcg_session_certify_cs();

                                        packet.Deserialize(list_stream[i].br);

                                        logger.Debug($"session_certify_cs : {packet.session_id} ");
                                        SetSession(packet.session_id);

                                    }
                                    ReadStreamPool.instance.Push(list_stream[i]);

                                }
                                else if ((ushort)WCS.Network.wce_cmd.session_open_sc == list_stream[i].command)
                                {
                                    //클라측에서 받는 부분
                                    {

                                        wcg_session_open_sc packet = new wcg_session_open_sc();

                                        packet.Deserialize(list_stream[i].br);

                                        logger.Debug($"session_open_sc : {packet.session_id} : {packet.result}");
                                        if(packet.result == wce_err.session_empty)
                                        {
                                            if (null != _clientTokenManager)
                                            {   // err
                                                _clientTokenManager.SessionCloseClient(packet.session_id, packet.result);
                                            }
                                            else
                                                logger.Error($"session_open_sc : _clientTokenManager == null");
                                        }
                                        else
                                            SetSession(packet.session_id);

                                    }
                                    // else {

                                    ReadStreamPool.instance.Push(list_stream[i]);

                                    //  }
                                }
                                else if ((ushort)WCS.Network.wce_cmd.session_heartbeat_sc == list_stream[i].command)
                                {
                                    if (null != _connectToken) //클라쪽에서 응답
                                    {
                                        wcg_session_heartbeat_cs packet = new wcg_session_heartbeat_cs();
                                        Send(packet);
                                        
                                    }
                                    ReadStreamPool.instance.Push(list_stream[i]);
                                }
                                else if ((ushort)WCS.Network.wce_cmd.session_heartbeat_cs == list_stream[i].command)
                                {
                                    if (null == _connectToken) //서버 측에서 받아서 
                                    {
                                        //wcg_session_heartbeat_cs packet = new wcg_session_heartbeat_cs();
                                        //packet.Deserialize(list_stream[i].br);
                                        _packetProcess(this, list_stream[i]);
                                        //WCS.logger.Info($"ReceiveProcess: session_heartbeat_cs ");
                                        //this._session?.RecvHeartBeat();


                                    }
                                    //ReadStreamPool.instance.Push(list_stream[i]);
                                }
                                else
                                {
                                    // this.PacketRecvTime = DateTime.Now.Ticks;
                                    _packetProcess(this, list_stream[i]);
                                }

                                _recv_count++;
                            }

                            
                        }
                    }

                    PostReceive();
                }
                else
                {
                    WCS.logger.Warn($"{_serverName}, {RecvEventArgs.SocketError}, transferred {RecvEventArgs.BytesTransferred}");
                    CloseNetwork();
                }
            }
        }

        public void SetSession(long session_id)
        {
            Interlocked.Exchange(ref _send_use_count, 0);
            bool reconnect_session = false;
            Session _session = null;
            //
            if (true == IsConnecter)
            {
                _session = SessionPool.instance.Pop();
            }
            else
            {
                if (0 < session_id)
                {
                    _session = SessionPool.instance.PopWait(session_id);
                   
                    if (null != _session)
                    {
                        reconnect_session = true;
                        logger.Warn($"reconnect_session {_session.SessionID} ");
                    }
                    else
                    {
                         
                        if (false == IsConnecter)
                        {
                            wcg_session_open_sc packet = new wcg_session_open_sc();
                            if (null == _session)
                                packet.result = (wce_err)wce_err.session_empty;
                            else

                                packet.result = (wce_err)0;

                            packet.session_id = session_id;

                            Send(packet);


                        }
                    }
                }
                else
                {

                    _session = SessionPool.instance.Pop();

                }
            }
            if (0 == session_id)
            {
                SessionID = WcatRandom.instance.Get();
            }
            else
            {
                SessionID = session_id;
            }
            if (null != _session)
            {
                _session.IsRpc = IsRpc;
                _session.Initialize(SessionID, _clientTokenManager, this, IsConnecter, reconnect_session);
            }
           

        }

        
        public static uint GetCheckSum(byte[] byPacket)
        {
            const int HEADER_SIZE = 4;
            uint checkSum = 0;
            int blockSize = (byPacket.Length - HEADER_SIZE) / 2;
            for (int i = 0; i < blockSize; i++)
            {
                int index = i * 2 + HEADER_SIZE;
                checkSum += (ushort)(byPacket[index] << 8);
                if (index + 1 < byPacket.Length)
                {
                    checkSum += (ushort)(byPacket[index + 1]);
                }
            }
            return checkSum;
        }

        public static void StructureToPtr<T>(byte[] dest, T source)
        {
            //Debug.Assert(dest != null);

            var gch = GCHandle.Alloc(dest, GCHandleType.Pinned);
            //Debug.Assert(gch != null);

            Marshal.StructureToPtr(source, gch.AddrOfPinnedObject(), false);
            gch.Free();
        }

        public static T PtrToStructure<T>(byte[] buffer, int size)
        {
            IntPtr pnt = Marshal.AllocHGlobal(size);

            Marshal.Copy(buffer, 0, pnt, size);

            return (T)Marshal.PtrToStructure(pnt, typeof(T));
        }
        public bool Send<T>(T packet) where T : class, ISerialize
        {
            if (true == this.socket?.Connected)
            {
                var stream = SendStreamPool.instance.CreateSendStream(packet);

                PostSend(stream);

                SendStreamPool.instance.Push(stream);

                return true;
            }

            return false;
        }

        public bool PostSend(SendStream stream)
        {
            if (false == this.socket?.Connected)
            {
                WCS.logger.Error($"{_serverName}, socket.Connected is false.");
                return false;
            }

            if (false == _send_buffer.Set(stream.buffer, stream.position))
            {
                WCS.logger.Error($"{_serverName}, _send_buffer.Set == false, {this.socket.RemoteEndPoint.ToString()}");
               
                CloseNetwork();
                return false;
            }

            if (0 < Interlocked.CompareExchange(ref _send_use_count, 1, 0))
            {
                if(this.socket != null)
                    WCS.logger.Warn($"{_serverName}, Interlocked.CompareExchange => {_send_use_count}, {this.socket.RemoteEndPoint.ToString()}");
                return true;
            }

            try
            {
                int read_pos;
                _send_request_size = _send_buffer.GetSendBuffer(out read_pos);

                if (0 < _send_request_size)
                {
                    SendEventArgs.SetBuffer(SendEventArgs.Offset, _send_request_size);

                    //Array.Copy(_send_buffer.buffer, read_pos, SendEventArgs.Buffer, SendEventArgs.Offset, _send_request_size);
                    Buffer.BlockCopy(_send_buffer.buffer, read_pos, SendEventArgs.Buffer, SendEventArgs.Offset, _send_request_size);
                    if (socket != null)
                    { 
                        if (false == socket.SendAsync(SendEventArgs))
                        {
                            SendProcess(SendEventArgs);
                        }
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                Interlocked.Exchange(ref _send_use_count, 0);
                WCS.logger.Error($"{_serverName}, SendAsync : {e.Message}");
            }

            return false;
        }

        private void PostSend(bool immediately, byte[] data, int size)
        {
            if (false == immediately)
            {
                if (false == _send_buffer.Set(data, size))
                {
                    WCS.logger.Error($"{_serverName}, PostSend error:_send_buffer data {data} : {size}"); 
                    CloseNetwork();
                    return;
                }

                if (0 < Interlocked.CompareExchange(ref _send_use_count, 1, 0))
                {
                    WCS.logger.Warn($"{_serverName},immediately immediately Interlocked. => {_send_use_count}, {this.socket.RemoteEndPoint.ToString()}");
                    return;
                }
            }

            try
            {
                // send 가 다 안되어서 나머지 보내는 경우
                if (true == immediately && 0 < size)
                {
                    _send_request_size -= size;

                    SendEventArgs.SetBuffer(SendEventArgs.Offset + size, _send_request_size);

                    if (false == socket.SendAsync(SendEventArgs))
                    {
                        SendProcess(SendEventArgs);
                    }
                }
                else
                {
                    int read_pos;
                    _send_request_size = _send_buffer.GetSendBuffer(out read_pos);

                    if (0 < _send_request_size)
                    {
                        SendEventArgs.SetBuffer(SendEventArgs.Offset, _send_request_size);

                        Buffer.BlockCopy(_send_buffer.buffer, read_pos, SendEventArgs.Buffer, SendEventArgs.Offset, _send_request_size);

                        if (false == socket.SendAsync(SendEventArgs))
                        {
                            SendProcess(SendEventArgs);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Interlocked.Exchange(ref _send_use_count, 0);
                WCS.logger.Error($"{_serverName}, SendAsync : {e.Message}");
            }
        }

        public void SendProcess(SocketAsyncEventArgs Args)
        {
            if (SocketError.Success == Args.SocketError)
            {
                // 다 못 보낸경우 처리.. 인데
                // asio async_write() 처럼 100% 전송을 보증하는지 아닌지.. 자료가 없다...
                if (_send_request_size > Args.BytesTransferred)
                {
                    WCS.logger.Info($"{_serverName}, _send_request_size : {_send_request_size}, transferred : {Args.BytesTransferred}");

                    PostSend(true, null, Args.BytesTransferred);
                }
                else
                {
                    if (0 < _send_buffer.GetDataSize)
                    {
                        //WCS.logger.Error($"SendProcess() => {_send_buffer.GetDataSize}, {this.socket.RemoteEndPoint.ToString()}");
                        PostSend(true, null, 0);
                    }
                    else
                    {
                        Interlocked.Exchange(ref _send_use_count, 0);
                        //WCS.logger.Error($"{_serverName}, Exchange {_send_use_count} ");
                    }
                }
            }
            else
            {
                WCS.logger.Error($"{_serverName}, error:{Args.SocketError}, transferred:{Args.BytesTransferred}");
                Interlocked.Exchange(ref _send_use_count, 0);
                CloseNetwork();
                
            }
        }
    }
}