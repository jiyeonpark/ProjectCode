#if SERVER_UNITY

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WCS.Network;
using WCS.Util;

namespace WCS.Network
{
    public abstract class NetworkBase
    {
       
        //protected delegate Task funcPacketProcess(int index, WCS.Network.ClientToken session, ReadStream stream);

        protected WCS.Network.SessionManager _sessionManager = null;
        //protected Dictionary<wce_cmd, funcPacketProcess> _dicPacketProcess = null;

        protected List<Task> _list_task = null;
        protected List<netContext> _list_netContext = null;
        protected List<ManualResetEvent> _list_event = null;
        protected bool _task_loop = true;
        protected readonly long _max_task = 0;
        protected long _task_index = 0;

        protected long _task_count = 0;
        protected  PacketHookHandler _packetHookHandler = null;
       
        public NetworkBase(int max_task)
        {
            _sessionManager = new WCS.Network.SessionManager();

            _list_task = new List<Task>();
            _list_netContext = new List<netContext>();
            _list_event = new List<ManualResetEvent>();

            _max_task = max_task;
        }

        public bool Initialize(string serverName, WCS.Config.network_ network_config, PacketHookHandler packetHookHandler,
            OnSessionOpened sessionOpenCallback = null, OnSessionClosed sessionCloseCallback = null, OnSocketOpened socketOpenCallback = null, OnSocketClosed socketCloseCallback = null, OnNetworkFault networkFaultCallback = null)
        {
            // _dicPacketProcess = new Dictionary<wce_cmd, funcPacketProcess>();

            _packetHookHandler = packetHookHandler;
            _sessionManager.Initialize(serverName, network_config, this.PacketProcess, sessionOpenCallback, sessionCloseCallback, socketOpenCallback, socketCloseCallback, networkFaultCallback);

            

            return true;
        }

       

        

        public void Start(WCS.Config.pool_ pool)
        {
            for (int i = 0; i < _max_task; i++)
            {
                _list_netContext.Add(new netContext(pool.default_size, pool.create_size));
                _list_event.Add(new ManualResetEvent(false));
                _list_task.Add(Run(i));
            }
        }

      


        public void Run(WCS.Config.server_ config)
        {
            _sessionManager.Run(config);
        }

        //public void RunUpdate(WCS.Config.server_ config_server, WCS.Config.rpc_ config_rpc, wre_server_type server_type)
        //{
        //    var serverinfo = new wrs_redis_serverinfo();
        //    {
        //        serverinfo.rpcinfo = new wrs_redis_rpcinfo();
        //        serverinfo.rpcinfo.type = server_type;
        //        serverinfo.rpcinfo.index = config_server.index;
        //        serverinfo.rpcinfo.ip = Encoding.UTF8.GetBytes(config_rpc.listen.ip);
        //        serverinfo.rpcinfo.port = (ushort)config_rpc.listen.port;

        //        //serverinfo.gameinfo = new wrs_redis_serverinfo();
        //        //serverinfo.gameinfo.rpcinfo = serverinfo.rpcinfo;
        //    }

        //    _sessionManager.RunUpdate(config_rpc, serverinfo);
        //}

        public void Stop()
        {
            _sessionManager.Stop();

            _task_loop = false;

            for (int i = 0; i < _list_task.Count; i++)
            {
                _list_event[i].Set();

                Task.Delay(1);
            }

            for (int i = 0; i < _list_task.Count; i++)
            {
                _list_task[i].Wait(1000);
            }
        }

        //public wrs_redis_rpcinfo GetRedisRpcinfo(wre_server_type type)
        //{
        //    return _rpcManager.GetRedisRpcinfo(type);
        //}

        public void PacketProcess( WCS.Network.ClientToken token, ReadStream stream)
        {
            int index = (int)(Interlocked.Increment(ref _task_index) % _max_task);
            _list_netContext[index].PushQueue(token, stream);
            _list_event[index].Set();
        }

        private Task Run(int index)
        {
            var task = Task.Run(async () =>
            {

                int next_time = DateTime.Now.ToUnixTimeInt() + 5;
                while (_task_loop)
                {
                    try
                    {
                        _list_event[index].WaitOne();

                        if (false == _task_loop)
                        {
                            break;
                        }

                        netContext.Context context = null;

                        _list_netContext[index].SwitchIndex();

                        //bool bEvent = false;
                        while (_list_netContext[index].PopQueue(out context))
                        {
                            if (context.token != null)
                            {
                                if (false == _packetHookHandler(context.token, context.stream))
                                {
                                    WCS.logger.Info($"!!!NetworkBase !!!_packetHookHandler{_task_count }");
                                }

                            }
                             //ReadStreamPool.instance.Push(context.stream);
                            _list_netContext[index].Push(context);
                            _task_count++;
                            

                        }

                        int current_time = DateTime.Now.ToUnixTimeInt();
                        if (current_time > next_time)
                        {

                           // WCS.logger.Info($"NETWorkBASE task_loop sec process: [{_task_count}] thread index: [{index}]");
                            _task_count = 0;
                            next_time = current_time + 5;
                        }
                        //if(!bEvent)
                            _list_event[index].Reset();
                    }
                    catch (Exception e)
                    {
                        WCS.logger.Error(e.ToString());

                        _list_event[index].Reset();
                    }
                }

                WCS.logger.Info($"thread exit. [{index}]");
            });

            return task;
        }

        public bool Send<T>( int session_id, T packet) where T : class, WCS.Network.ISerialize
        {
            var session = _sessionManager.GetSession(session_id);
            if (null != session )
            {
                return session.Send(packet);
            }

            return false;
        }

        public bool Send( long session_id, SendStream stream)
        {
            var session = _sessionManager.GetSession(session_id);
            if (null != session)
            {
                return session.PostSend(stream);
            }

            return false;
        }

        //public bool Send<T>( T packet) where T : class, WCS.Network.ISerialize
        //{
           
        //    //if (null != rpc && null != rpc.clientToken)
        //    //{
        //    //    return rpc.clientToken.PostSend(packet);
        //    //}

        //    return false;
        //}

        public bool SendAll<T>(wre_server_type type, T packet) where T : class, WCS.Network.ISerialize
        {
            SendStream stream = SendStreamPool.instance.CreateSendStream(packet);

            bool result = SendAll(stream);

            SendStreamPool.instance.Push(stream);

            return result;
        }

        public bool SendAll( SendStream stream)
        {
            bool result = false;
            var list = _sessionManager.GetSessionAll();

            //if (null != list)
            //{
            foreach (var session in list)
            {
                if (null != session.Value && null != session.Value.clientToken)
                {
                    session.Value.clientToken.PostSend(stream);
                    result = true;
                }
            }
            //}

            return result;
        }        

        public void BroadCastPacketWorld<T>(T packet) where T : class, WCS.Network.ISerialize
        {
            SendStream stream = SendStreamPool.instance.CreateSendStream(packet);

            SendAll(stream);
         
            SendStreamPool.instance.Push(stream);
        }

        //public ClientToken GetSession(int session_id)
        //{
        //    return _sessionManager.GetSession(session_id);
        //}
       
    }
}

#endif