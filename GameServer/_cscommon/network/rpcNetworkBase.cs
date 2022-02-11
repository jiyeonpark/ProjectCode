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
    public abstract class RpcNetworkBase
    {
        public static int OBSERVER_CONNECT_CHECKTIME = 1;
        public static int OBSERVER_CONNECT_TIMEOUT = 60;

        protected delegate Task funcPacketProcess(int index, WCS.Network.ClientToken session, ReadStream stream);
        
        protected WCS.Network.RpcManager _rpcManager = null;
        protected Dictionary<wre_cmd, funcPacketProcess> _dicPacketProcess = null;

        protected List<Task> _list_task = null;
        protected List<rpcContext> _list_netContext = null;
        protected List<ManualResetEvent> _list_event = null;
        protected bool _task_loop = true;
        protected readonly long _max_task = 0;
        protected long _task_index = 0;

        protected long _task_count = 0;
        public bool ConnectedObserver { get; set; }
        public bool RequestRoomData { get; set; }
        public RpcNetworkBase(int max_task)
        {
            
            _rpcManager = new WCS.Network.RpcManager();

            _list_task = new List<Task>();
            _list_netContext = new List<rpcContext>();
            _list_event = new List<ManualResetEvent>();

            _max_task = max_task;
        }

        public bool Initialize(string serverName, WCS.Config.rpc_ rpc_config, ConnectedCallback connectedCallback = null, RpcManager.SendRedisRpcinfo sendRedisRpcinfo = null, RpcManager.RemoveManagedServer removeManagedServer = null)
        {
            _dicPacketProcess = new Dictionary<wre_cmd, funcPacketProcess>();

            _rpcManager.Initialize(serverName, rpc_config, this.RpcPacketProcess, connectedCallback, sendRedisRpcinfo, removeManagedServer);

           RegisterPacketProcess();

            return true;
        }

        public bool InitializeRedis(WCS.Config.redis_ config)
        {
            return _rpcManager.InitializeRedis(config);
        }

        protected abstract void RegisterPacketProcess();

        public void Start(WCS.Config.pool_ pool)
        {
            for (int i = 0; i < _max_task; i++)
            {
                _list_netContext.Add(new rpcContext(pool.default_size, pool.create_size));
                _list_event.Add(new ManualResetEvent(false));
                _list_task.Add(Run(i));
            }
        }

        public void ConnectRpcServer(List<wrs_redis_rpcinfo> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                _rpcManager.ConnectRpcServer(list[i].type, list[i]);
            }
        }
        public void ConnectRpcServer(WCS.Config.observer_ config)
        {
            var observer = new wrs_redis_rpcinfo();
            {
                observer.type = wre_server_type.gmobserver;
                observer.ip = config.host.ip;
                observer.port = (ushort)config.host.port;
            }

            _rpcManager.ConnectRpcServer(observer.type, observer);
        }

        public void ConnectRpcServer(wrs_redis_rpcinfo rpcinfo)
        {
            _rpcManager.ConnectRpcServer(rpcinfo.type, rpcinfo);
        }

        public void UpdateRedisRpcinfo(List<wrs_redis_rpcinfo> list)
        {
            _rpcManager.SetRedisRpcinfo(list);
        }

        public void Run(WCS.Config.rpc_ config, wrs_redis_rpcinfo rpcinfo)
        {
            _rpcManager.Run(config, rpcinfo);
        }
        public void RunUpdate(WCS.Config.server_ config_server, WCS.Config.network_ config_game, WCS.Config.rpc_ config_rpc, wre_server_type server_type)
        {
            var serverinfo = new wrs_redis_serverinfo();
            {
                serverinfo.rpcinfo = new wrs_redis_rpcinfo();
                serverinfo.rpcinfo.type = server_type;
                serverinfo.rpcinfo.index = config_server.index;
                serverinfo.rpcinfo.ip = config_rpc.listen.ip;
                serverinfo.rpcinfo.port = (ushort)config_rpc.listen.port;

                serverinfo.gameserver_info = new wrs_redis_gameserver();
                serverinfo.gameserver_info.state = (byte)config_server.state;
                serverinfo.gameserver_info.rpcinfo = new wrs_redis_rpcinfo();
                serverinfo.gameserver_info.rpcinfo.index = config_server.index;
                serverinfo.gameserver_info.rpcinfo.type = wre_server_type.game;
                serverinfo.gameserver_info.rpcinfo.ip = config_game.listen.ip;
                serverinfo.gameserver_info.rpcinfo.port = (ushort)config_game.listen.port;
            }

            _rpcManager.RunUpdate(config_rpc, serverinfo);
        }
        public void RunUpdate(WCS.Config.server_ config_server, WCS.Config.rpc_ config_rpc, wre_server_type server_type)
        {
            var serverinfo = new wrs_redis_serverinfo();
            {
                serverinfo.rpcinfo = new wrs_redis_rpcinfo();
                serverinfo.rpcinfo.type = server_type;
                serverinfo.rpcinfo.index = config_server.index;
                serverinfo.rpcinfo.ip = config_rpc.listen.ip;
                serverinfo.rpcinfo.port = (ushort)config_rpc.listen.port;

                serverinfo.gameserver_info = new wrs_redis_gameserver();
                serverinfo.gameserver_info.rpcinfo = serverinfo.rpcinfo;
            }

            _rpcManager.RunUpdate(config_rpc, serverinfo);
        }

        public void Stop()
        {
            _rpcManager.Stop();

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

        public void RpcPacketProcess(WCS.Network.ClientToken session, ReadStream stream)
        {
            int index = (int)(Interlocked.Increment(ref _task_index) % _max_task);
            _list_netContext[index].PushQueue(session, stream);
            _list_event[index].Set();
        }

        private Task Run(int index)
        {
            var task = Task.Run(async () =>
            {
               
                int next_time =  DateTime.Now.ToUnixTimeInt() + 5;
                while (_task_loop)
                {
                    try
                    {
                        _list_event[index].WaitOne();

                        if (false == _task_loop)
                        {
                            break;
                        }

                        rpcContext.Context context = null;

                        _list_netContext[index].SwitchIndex();


                        while (_list_netContext[index].PopQueue(out context))
                        {
                            funcPacketProcess func = null;

                            if (_dicPacketProcess.TryGetValue((wre_cmd)context.stream.command, out func))
                            {
                                await func(index, context.session, context.stream).ConfigureAwait(false);
                            }
                            else
                            {
                                WCS.logger.Error($"packet process not found. [{context.stream.command}]");
                            }

                            //ReadStreamPool.instance.Push(context.stream);
                            _list_netContext[index].Push(context);
                            _task_count++;
                        }

                        int current_time = DateTime.Now.ToUnixTimeInt();
                        if (current_time > next_time)
                        {

                            //WCS.logger.Info($"task_loop sec process: [{_task_count}] thread index: [{index}]");
                            _task_count = 0;
                            next_time = current_time + 5;
                        }
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

        public bool RpcSend<T>(wre_server_type type, int index, T packet) where T : class, WCS.Network.ISerialize
        {
            var rpc = _rpcManager.GetRpc(type, index);
            if (null != rpc && null != rpc.clientToken)
            {
                return rpc.clientToken.Send(packet);
            }

            return false;
        }

        public bool RpcSend(wre_server_type type, int index, SendStream stream)
        {
            var rpc = _rpcManager.GetRpc(type, index);
            if (null != rpc && null != rpc.clientToken)
            {
                return rpc.clientToken.PostSend(stream);
            }

            return false;
        }

        public bool RpcSend<T>(wre_server_type type, T packet) where T : class, WCS.Network.ISerialize
        {
            var rpc = _rpcManager.GetRpc(type);
            if (null != rpc && null != rpc.clientToken)
            {
                return rpc.clientToken.Send(packet);
            }

            return false;
        }

        public bool RpcSend(wre_server_type type, SendStream stream)
        {
            var rpc = _rpcManager.GetRpc(type);
            if (null != rpc && null != rpc.clientToken)
            {
                return rpc.clientToken.PostSend(stream);
            }

            return false;
        }

        public bool RpcSendAll<T>(wre_server_type type, T packet) where T : class, WCS.Network.ISerialize
        {
            SendStream stream = SendStreamPool.instance.CreateSendStream(packet);

            bool result = RpcSendAll(type, stream);

            SendStreamPool.instance.Push(stream);

            return result;
        }

        public bool RpcSendAll(wre_server_type type, SendStream stream)
        {
            bool result = false;
            var list = _rpcManager.GetRpcAll(type);

            if (null != list)
            {
                foreach (var rpc in list)
                {
                    if (null != rpc && null != rpc.clientToken)
                    {
                        rpc.clientToken.PostSend(stream);
                        result = true;
                    }
                }
            }

            return result;
        }        

        public void BroadCastPacketWorld<T>(T packet) where T : class, WCS.Network.ISerialize
        {
            SendStream stream = SendStreamPool.instance.CreateSendStream(packet);

            RpcSendAll(wre_server_type.game, stream);
            //RpcSendAll(wre_server_type.match, stream);

            SendStreamPool.instance.Push(stream);
        }

        public RpcToken GetRpc(wre_server_type type, int index)
        {
            return _rpcManager.GetRpc(type, index);
        }

        public List<RpcToken> GetRpcAll(wre_server_type type)
        {
            return _rpcManager.GetRpcAll(type);
        }

        public RpcToken GetRpc(wre_server_type type)
        {
            return _rpcManager.GetRpc(type);
        }
        public bool RpcSend<T>(long session_id, T packet) where T : class, WCS.Network.ISerialize
        {
            var session = _rpcManager.GetSession(session_id);
            if (null != session)
            {
                return session.Send(packet);
            }

            return false;
        }
        //public RpcToken GetRpcOne(wre_server_type type)
        //{
        //    return _rpcManager.GetRpc(type);
        //}
    }
}

#endif