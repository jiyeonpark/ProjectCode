using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using WCS.Util;

namespace WCS.Network
{
    public class Session : WCS.IResetable
    {
        private ClientTokenManager _clientTokenManager = null;
        protected readonly IPEndPoint _endpoint = null;
        protected ClientToken _clientToken = null;
        protected bool _connected = false;

        protected bool _opened = false;
        protected bool _is_connector = false;

        protected long  _timeout = 0;
        private long    _connector_session_id = 0;
        private bool    _isRpc = false;
        private bool    _force_disconnect = false;
        private InGameState _game_state = 0;
        private int     _warn_heartbeat = 0;
        private bool    _old_back_ground = false;
        private bool    _back_ground = false;
        private bool    _run_heartbeat = false;
        public HeartbeatManager _heartbeatManager = null;
        private ConcurrentQueue<HeartbeatTask> _queueTask = null;

        
        public void Reset()
        {
#if SERVER_UNITY
            _queueTask.Clear();
            _warn_heartbeat = 0;
            _Run_HeartBeat = false;

            _old_back_ground = false;
            _back_ground = false;
#else
            ClearQ();
#endif

            _connected = false;
            _force_disconnect = false;
            _game_state = WCS.InGameState.none;
        }
        public void Release()
        {
#if SERVER_UNITY
            _queueTask.Clear();
            _warn_heartbeat = 0;
            _Run_HeartBeat = false;
            _old_back_ground = false;
            _back_ground = false;
#else
            ClearQ();
#endif
        }
        public Socket socket
        {
            get { return _clientToken.socket; }
        }
        public ClientToken clientToken
        {
            get { return _clientToken; }
            set { _clientToken = value; }
        }
        public IPEndPoint endpoint
        {
            get { return _endpoint; }
        }
        public bool _IsOldBackGround
        {
            get { return _old_back_ground; }
            set { _old_back_ground = value; }
        }
        public bool _IsBackGround
        {
            get { return _back_ground; }
            set { _back_ground = value; }
        }
        
        public int _Warn_HeartBeat
        {
            get { return _warn_heartbeat; }
            set { _warn_heartbeat = value; }
        }

        public bool _Run_HeartBeat
        {
            get { return _run_heartbeat; }
            set { _run_heartbeat = value; }
        }
        public bool IsOpened
        {
            get { return _opened; }
            set { _opened = value; }
        }

        public bool IsRpc
        {
            get { return _isRpc; }
            set { _isRpc = value; }
        }


        public bool IsConnected()
        {
            if (null != _clientToken && null != _clientToken.socket)
            {
                return _clientToken.socket.Connected;
            }
            return false;
        }
        public bool IsForceDisconnect
        {
            get { return _force_disconnect; }
            set { _force_disconnect = value; }
        }
        public long SessionID
        {
            get { return _connector_session_id; }
            set { _connector_session_id = value; }
        }
        public WCS.InGameState inGameState
        {
            get { return _game_state; }
            set { _game_state = value; }
        }

        public Session()
        {
            _heartbeatManager = new HeartbeatManager();
            _queueTask = new ConcurrentQueue<HeartbeatTask>();
            _Run_HeartBeat = false;
            _Warn_HeartBeat = 0;
            _IsOldBackGround = false;
            _IsBackGround = false;
        }
        public void Initialize(long session_id, ClientTokenManager clientTokenManager_, ClientToken socket, bool is_connector, bool reconnect_session)
        {
            
           
            _opened = true;
            _is_connector = is_connector;
#if SERVER_UNITY
            if (clientToken != null)
            {
                //unsafe
                //{
                //    TypedReference tr1 = __makeref(_clientToken);
                //    TypedReference tr2 = __makeref(socket);
                //    IntPtr ptr1 = **(IntPtr**)(&tr1);
                //    IntPtr ptr2 = **(IntPtr**)(&tr2);
                //    WCS.logger.Debug($"old {clientToken.TokenId }pointer:{ptr1} / new clientToken {socket.TokenId} pointer:{ptr2}");
                //}
                WCS.logger.Debug($"old {clientToken.TokenId }/ new clientToken {socket.TokenId} ");
            }
#endif
            clientToken = socket;
            _clientTokenManager = clientTokenManager_;
#if SERVER_UNITY
            _Run_HeartBeat = false;
            _queueTask.Clear();
            _Warn_HeartBeat = 0;
            _old_back_ground = false;
            _back_ground = false;
#else
            ClearQ();
#endif
            //if (0 == session_id)
            //{
            //    SessionID = WcatRandom.instance.Get();
            //}
            //else
            {
                SessionID = session_id;
            }

            if (false == _is_connector)
            {
                //서버만 보내준다.
                var message = new WCS.Network.wcg_session_open_sc();
                if (reconnect_session)
                {
                    message.result = (wce_err)99;
                }
                else
                {
                    message.result = (wce_err)0;
                }

                message.session_id = SessionID;
                Send(message);
                                
            }

            
            if (null != _clientTokenManager)
            {   //서버이면
                _clientTokenManager.SessionOpenClient(this, reconnect_session);
                
            }
            
        }

        public bool Send<T>(InGameState index, T packet) where T : class, WCS.Network.ISerialize
        {
            if (null != clientToken && IsOpened)
            {
                var stream = SendStreamPool.instance.CreateSendStream(packet);

                clientToken.PostSend(stream);

                SendStreamPool.instance.Push(stream);
                
                
                return true;
            }

            return false;
            
        }
        public bool Send<T>(T packet) where T : class, ISerialize
        {
            bool ret = false;
            if (null != clientToken && IsOpened)
            {
               
                var stream = SendStreamPool.instance.CreateSendStream(packet);

                ret = clientToken.PostSend(stream);

                SendStreamPool.instance.Push(stream);

                return ret;
                
            }

            return ret;
        }

        public void SetForceDisconnect()
        {
            //var addr = socket.RemoteEndPoint;
            WCS.logger.Info($"CSocket::SetForceDisconnect() socket id : ");
            _connected = false;
            _force_disconnect = true;


        }
        public bool CheckHeartbeat()
        {
            //선중단 같은 연결 끊기 감지!
            if (clientToken == null)
                return false;

            if(_Run_HeartBeat == false)
            {
#if SERVER_UNITY
                _queueTask.Clear();
#else
                ClearQ();
#endif
                //WCS.logger.Debug($"Heartbeat Stop {SessionID}");
                return true;
            }

            if(_IsOldBackGround == true && _IsBackGround == false)
            {
#if SERVER_UNITY
                _queueTask.Clear();
#else
                ClearQ();
#endif
                _Warn_HeartBeat = 0;
                _IsOldBackGround = _IsBackGround;
            }
            int delay = 2;
            if (_IsBackGround == true)
                delay = 60;
            ////백그라운드 갈때 중지?
            ////바로 정리
            if (_Warn_HeartBeat >= delay && clientToken != null && clientToken.IsConnected)
            {
                _Warn_HeartBeat = 0;
                _Run_HeartBeat = false;


                return false;
            }
            

            if (_heartbeatManager.ProcessTask(_IsBackGround, DateTime.Now.ToUnixTimeInt(), _game_state, ref _queueTask) > 0)
            {

                if (clientToken != null && clientToken.IsConnected)
                {
                    if (_queueTask.Count > 0)
                    {
                        SendHeartBeat();
                        if (_IsBackGround)
                        {
                            if (_queueTask.Count > 2)
                            {
                                _Warn_HeartBeat += 1;
#if SERVER_UNITY
                                //_queueTask.Clear();
#else
                ClearQ();
#endif
                            }
                        }
                        else
                        {
                            if (_queueTask.Count > 2)
                                _Warn_HeartBeat += 1;
                        }

                    }
                    
                }

            }
            else
            {
                // WCS.logger.Info($"CheckHeartbeat::ProcessTask()  size ==0");
            }
            
           
            return true;
        }
        public bool PopQueue(out HeartbeatTask hbTasks)
        {
            
            return _queueTask.TryDequeue(out hbTasks);
        }
        public void Destroy()
        {
            if (null != _clientTokenManager)
            {
                if (false == _is_connector)
                {
                    //서버만 보내준다.
                    //var message = new WCS.Network.wcg_session_close_sc();
                    
                    //Send(message);
                }
               
                if(clientToken != null && clientToken.IsConnected)
                {
                    clientToken.CloseNetwork();
                }
                SetForceDisconnect();
#if SERVER_UNITY
                _queueTask.Clear();
#else
                ClearQ();
#endif
                //_clientTokenManager.SessionCloseClient(this);
            }

            //SessionID = 0;

            //Reset();

        }


        public int SendHeartBeat()
        {
            int ret = 0;
            if (null != _clientTokenManager)
            {
                if (false == _is_connector)
                {
                    //서버만 보내준다.
                    var message = new WCS.Network.wcg_session_heartbeat_sc();

                    Send(message);
                    
                    return ret;
                }
                else
                {
                    var message = new WCS.Network.wcg_session_heartbeat_cs();

                    Send(message);
                    
                }
                
            }
            else
                logger.Error("Send Heartbeat FAIL ");

            return ret;

        }

        public void RecvHeartBeat()
        {
            if (null != _clientTokenManager)
            {
                if (false == _is_connector)
                {
                    HeartbeatTask _task = null;
                    while (PopQueue(out _task))
                    {
                        //logger.Warn($"Recv RecvHeartBeat  Sessionid = {SessionID} _task{_task.mCommand} , {_task.mArgs.ToString()} ");
                        //break;
                    }
                }
                else
                {

                }

            }

        }

#if !SERVER_UNITY
        // ConcurrentQueue Clear() 대체함수 : .Net4.x(Unity) 버전으로 인해 Clear() 를 못씀
        public void ClearQ()
        {
            HeartbeatTask _task = null;
            while (PopQueue(out _task)) { }
        }
#endif

    }
}