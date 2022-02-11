#if SERVER_UNITY

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WCS.Util;

namespace WCS.Network
{
    public class waitItem
    {
        public long expire_time = 0;
        public long session_id = 0;
    }
    public class SessionManager
    {
      
        private ClientTokenManager _clientTokenManager = null;
        private SendBufferPool _sendBufferPool = null;
        private Acceptor _acceptor = null;
   
        private ConcurrentDictionary<long, Session> _dicSession = null;
        
        private ConcurrentDictionary<ClientToken, ClientToken> _dicAcceptToken = null;
        private CancellationTokenSource _tokenSource = null;
        
        private object _lock_dic_server_type = null;
      
        private bool _loop = true;
   
        private string _serverName = string.Empty;

        private OnSessionOpened _sessionOpenCallback = null;
        private OnSessionClosed _sessionCloseCallback = null;
        private OnSocketOpened _socketOpenCallback = null;
        private OnSocketClosed _socketCloseCallback = null;
        private OnNetworkFault _networkFaultCallback = null;
        public ConcurrentDictionary<long, waitItem> _map_wait = null;
          

        public void Initialize(string serverName, WCS.Config.network_ network_config, PacketProcess packetProcess,
            OnSessionOpened session_open = null, OnSessionClosed session_closed = null,
            OnSocketOpened socket_open = null, OnSocketClosed socket_closed = null, OnNetworkFault network_fault= null )
        {
            
            _tokenSource = new CancellationTokenSource();
            _clientTokenManager = new ClientTokenManager();
            _sendBufferPool = new SendBufferPool();
            _acceptor = new Acceptor(serverName, _clientTokenManager);
           
          
            _dicSession = new ConcurrentDictionary<long, Session>();
          
            _dicAcceptToken = new ConcurrentDictionary<ClientToken, ClientToken>();

            _map_wait = new ConcurrentDictionary<long, waitItem>();
                        

            _lock_dic_server_type = new object();

            _acceptor.Initialize(network_config.listen.port);            

            _sendBufferPool.Initialize(network_config.max_session);

            int max_session = network_config.max_session ;   // acceptor

            _clientTokenManager.Initialize(serverName, max_session, _sendBufferPool, false);
            _clientTokenManager.CreateEventArgs(packetProcess);

            _clientTokenManager.AcceptedCallback += this.Accepted;
            _clientTokenManager.DisConnectedAcceptCallback += this.DisConnectedAccept;
            _clientTokenManager.SessionOpenCallback += this.SetSession;
            _clientTokenManager.SessionCloseCallback += this.CloseSession;
            _socketOpenCallback += socket_open;
            _socketCloseCallback += socket_closed;
            _sessionOpenCallback += session_open;
            _sessionCloseCallback += session_closed;

            _networkFaultCallback += network_fault;
            _serverName = serverName;
        }

        
       
        public void Run(WCS.Config.server_ config)
        {   
          
            _acceptor.Start();

          var task = Task.Run(async () =>
          {                  
                while (_loop)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500), _tokenSource.Token).ConfigureAwait(false);

                    if (false == _loop)
                    {
                        break;
                    }

                    try
                    {

                        long now = WCS.Util.Util.GetTickCount64();

                        foreach (var wait_session in _map_wait)
                        {
                            long wait_time = wait_session.Value.expire_time;

                         

                            if (now > wait_time)
                            {
                                // wait session delete
                                var session = SessionPool.instance.PopWait(wait_session.Key);

                                if (null != session && session.inGameState != InGameState.none)
                                {
                                    session.Destroy();
                                    CloseSession(session.SessionID, 0);
                                    WCS.logger.Warn($"Destroy !!!wait_session PopWait :now {now} , wait_time {wait_time}, Key {wait_session.Key}");
                                }

                                   
                                _map_wait.TryRemove(wait_session.Key, out _);
                                break;
                            }


                        }

                      foreach (var session in _dicSession)
                      {
                          if (false == session.Value.CheckHeartbeat())
                          {

                              //if (session.Value.IsForceDisconnect == true)
                              {
                                  if (session.Value.clientToken != null)
                                  {
                                      if(session.Value.clientToken.IsConnected)
                                      {
                                          WCS.logger.Warn($"CheckHeartbeat !!! session_id {session.Value.clientToken.SessionID} ");

                                         session.Value.clientToken.CloseNetwork();
                                      }

                                      if (null != _networkFaultCallback)
                                      {

                                          _networkFaultCallback(session.Value);
                                          session.Value._Run_HeartBeat = false;
                                      }
                                  }
                              }

                          }

                      }


                  }
                    catch (System.Exception e)
                    {
                        WCS.logger.Error(e.ToString());
                    }
                }
          });
        }

        public bool RemoveWaitSession(long session_id)
        {
            waitItem wait = null;
            if(_map_wait.TryRemove(session_id, out wait))
            {
                WCS.logger.Info($"Remove waitItem : {session_id}");
                return  true;
                
            }
            
            return false;
            
        }
        public void Stop()
        {
            _loop = false;

            _acceptor.Stop();
            _dicSession.Clear();
            if (null != _tokenSource)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
            }
        }

    

        public void Accepted(ClientToken token)
        {  
            //var token = session as Session;
            WCS.logger.Info($"Accepted Socket : {token?.socket?.RemoteEndPoint}");
            _dicAcceptToken.TryAdd(token, token);
            if (null != _socketOpenCallback)
            {
                _socketOpenCallback(token);
            }

        }

        public void SetSession(Session session, bool ReConnect = false)
        {
            if (null != _sessionOpenCallback)
            {
                RemoveWaitSession(session.SessionID);
              
                _sessionOpenCallback(session, ReConnect);

                SessionPool.instance.PushWait(session);

                Session tempToken;
                if (!_dicSession.TryGetValue(session.SessionID, out tempToken))
                {
                    _dicSession.TryAdd(session.SessionID, session);
                }
                session._Run_HeartBeat = true;
                WCS.logger.Info($"SetSession Start HeartBeat: {session.SessionID} ");
                //session.SendHeartBeat();
            }
            
        }
        public void CloseSession(long SessionID, wce_err err)
        {
            if (SessionID != 0)
            {
                Session session = null;
                if (_dicSession.TryGetValue(SessionID, out session))
                {

                    if (session != null)
                    {
                        if (session.IsForceDisconnect == true)
                        {
                            session.inGameState = InGameState.none;
                            if (null != _sessionCloseCallback)
                            {
                                _sessionCloseCallback(session);
                            }

                            Session temp = null;
                            _dicSession.TryRemove(session.SessionID, out temp);
                            session.Reset();
                        }
                        else
                        {

                            if (session.inGameState == InGameState.play)
                            {
                                waitItem wait = new waitItem();
                                wait.session_id = session.SessionID;
                                wait.expire_time = WCS.Util.Util.GetTickCount64() + 30000;
                                _map_wait.TryAdd(session.SessionID, wait);
                                // SessionPool.instance.PushWait(session);
                                WCS.logger.Info($"PushWait AddWaitSession : {session.SessionID} wait.expire_time{ wait.expire_time}");
                                
                            }
                            else
                            {
                                if (null != _sessionCloseCallback)
                                {
                                    _sessionCloseCallback(session);
                                }

                                SessionPool.instance.PopWait(session.SessionID);
                                Session temp = null;
                                _dicSession.TryRemove(session.SessionID, out temp);
                                session.Reset();
                            }


                        }
                    }
                }
            }
                
            //if (session != null)
            //{
            //    if (session.IsForceDisconnect == true)
            //    {

            //        session.inGameState = InGameState.none;
            //        if (null != _sessionCloseCallback)
            //        {
            //            _sessionCloseCallback(session);
            //        }
                    
            //        Session temp = null;
            //        _dicSession.TryRemove(session.SessionID, out temp);
            //        session.Reset();
            //    }
            //    else
            //    {

            //        if (session.inGameState < InGameState.load_complate || session.inGameState >= InGameState.game_end)
            //        {
                        
            //            if (null != _sessionCloseCallback)
            //            {
            //                _sessionCloseCallback(session);
            //            }

            //            SessionPool.instance.PopWait(session.SessionID);
            //            Session temp = null;
            //            _dicSession.TryRemove(session.SessionID, out temp);
            //            session.Reset();

                       

            //        }
            //        else
            //        {
            //            waitItem wait = new waitItem();
            //            wait.session_id = session.SessionID;
            //            wait.expire_time = WCS.Util.Util.GetTickCount64() + 30000;
            //            _map_wait.TryAdd(session.SessionID, wait);
            //           // SessionPool.instance.PushWait(session);
            //            WCS.logger.Info($"PushWait AddWaitSession : {session.SessionID} wait.expire_time{ wait.expire_time}");
            //        }

                   
            //    }
            //}
        }

       
        public void DisConnectedAccept(ClientToken token)
        {
           
            ClientToken temp = null;
            if (_dicAcceptToken.TryRemove(token, out temp))
            {
                //if (null != temp && null != _removeManagedServer)
                //{
                //    _removeManagedServer(token);
                //}
                if (null != _socketCloseCallback)
                {
                   
                    _socketCloseCallback(token);
                    
                    if (token != null)
                    {
                        CloseSession(token.SessionID, 0);
                    }
                }
            }
        }

        
        public ClientToken GetSession(long session_id)
        {
            Session token = null;
            _dicSession.TryGetValue(session_id, out token);

            if (null == token)
            {
                WCS.logger.Error($"session not found. {session_id}");
            }

            return token.clientToken;
        }
        public ConcurrentDictionary<long,Session> GetSessionAll()
        {            

            return _dicSession;
        }
        //public List<Session> GetSessionAll()
        //{
        //    //List<Session> list = _dicSession.ToList<Session>();
        //    //if (_dicSession.TryGetValue(type, out list))
        //    //{
        //    //    lock (_lock_dic_server_type)
        //    //    {
        //    //        return list.ToList();
        //    //    }
        //    //}

        //    //return list.ToList();
        //}
    }
}

#endif