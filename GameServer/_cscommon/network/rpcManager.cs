#if SERVER_UNITY

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WCS.Util;

namespace WCS.Network
{
    public class RpcManager
    {
        public delegate void SendRedisRpcinfo(List<wrs_redis_rpcinfo> list);
        public delegate void RemoveManagedServer(ClientToken token);

        private ClientTokenManager _clientTokenManager = null;
        private SendBufferPool _sendBufferPool = null;
        private Acceptor _acceptor = null;
        private Connector _connector = null;
        private ConcurrentDictionary<string, int> _dicConnectTry = null;
        private ConcurrentDictionary<string, RpcToken> _dicFullTag = null;
        private ConcurrentDictionary<wre_server_type, List<RpcToken>> _dicServerType = null;
        private ConcurrentDictionary<ClientToken, ClientToken> _dicAcceptToken = null;
        private CancellationTokenSource _tokenSource = null;
        private ConnectedCallback _connectedCallback = null;
        private object _lock_dic_server_type = null;
        private Redis.RedisRpcserver _redis = null;
        private bool _loop = true;
        private wrs_redis_rpcinfo _redis_rpcinfo_observer = null;
        private wrs_redis_rpcinfo _redis_rpcinfo_self = null;
        private ConcurrentQueue<List<wrs_redis_rpcinfo>> _queue_connection_rpcinfo = null;
        private SendRedisRpcinfo _sendRedisRpcinfo = null;
        private RemoveManagedServer _removeManagedServer = null;
        private string _serverName = string.Empty;
        private ConcurrentDictionary<long, ClientToken> _dicSession = null;

        public void Initialize(string serverName, WCS.Config.rpc_ config_rpc, rpcPacketProcess packetProcess, ConnectedCallback connectedCallback = null, SendRedisRpcinfo sendRedisRpcinfo = null, RemoveManagedServer removeManagedServer = null, Redis.RedisRpcserver redis = null)
        {
            _tokenSource = new CancellationTokenSource();
            _clientTokenManager = new ClientTokenManager();
            _sendBufferPool = new SendBufferPool();
            _acceptor = new Acceptor(serverName, _clientTokenManager);
            _connector = new Connector(serverName, _clientTokenManager);
            _dicConnectTry = new ConcurrentDictionary<string, int>();
            _dicFullTag = new ConcurrentDictionary<string, RpcToken>();
            _dicServerType = new ConcurrentDictionary<wre_server_type, List<RpcToken>>();
            _dicAcceptToken = new ConcurrentDictionary<ClientToken, ClientToken>();
            _queue_connection_rpcinfo = new ConcurrentQueue<List<wrs_redis_rpcinfo>>();
            _lock_dic_server_type = new object();
            _dicSession = new ConcurrentDictionary<long, ClientToken>();

            _acceptor.Initialize(config_rpc.listen.port);            

            _sendBufferPool.Initialize(config_rpc.max_session);
                       
            int max_session = config_rpc.max_session * 2;   // acceptor, connector 같이 쓰기 때문에

            _clientTokenManager.Initialize(serverName, max_session, _sendBufferPool,true);
            _clientTokenManager.CreateEventArgs_rpc(packetProcess);

            _clientTokenManager.AcceptedCallback += this.Accepted;
            _clientTokenManager.DisConnectedAcceptCallback += this.DisConnectedAccept;
            _clientTokenManager.ConnectedCallback += this.Connected;
            _clientTokenManager.ConnecFailCallback += this.ConnectFail;
            _clientTokenManager.DisConnectedCallback += this.DisConnected;

            _connectedCallback = connectedCallback;
            _sendRedisRpcinfo = sendRedisRpcinfo;
            _removeManagedServer = removeManagedServer;

            _serverName = serverName;
        }

        public bool InitializeRedis(Config.redis_ config)
        {
            _redis = new Redis.RedisRpcserver();

            return _redis.Initialize(config);
        }

        // gm server는 gm observer 정보만 획득한다
        // gm observer는 모든 정보를 수집한다.
        public void Run(Config.rpc_ config, wrs_redis_rpcinfo rpcinfo)
        {   
            int delay_sec_time = config.update_sec;
            int update_time_out = config.update_timeout_sec;

            _acceptor.Start();
            
			var task = Task.Run ( async() => 
            {
                var list = new List<wrs_redis_rpcinfo>();
                
                logger.Info(Newtonsoft.Json.JsonConvert.SerializeObject(rpcinfo, Newtonsoft.Json.Formatting.Indented));

                string self_fullTag = $"{define.SERVER_TAG_NAME[(int)rpcinfo.type]}_{rpcinfo.index}";

                while (_loop)
                {
                    await Task.Delay(TimeSpan.FromSeconds(delay_sec_time), _tokenSource.Token).ConfigureAwait(false);

                    if (!_loop)
						break;

                    try
                    {
                        rpcinfo.update_time = DateTime.Now.ToUnixTimeInt();
                        await _redis.SetHashDataStreamAsync(Redis.RedisRpcserver.KEY_RPC_INFO, self_fullTag, rpcinfo).ConfigureAwait(false);

                        list.Clear();

                        // get rpc servers info
                        var redis_data = await _redis.GetHashDataAllToDictionaryStreamAsync<wrs_redis_rpcinfo>(Redis.RedisRpcserver.KEY_RPC_INFO).ConfigureAwait(false);
                        if (redis_data != null) {
                            int now_time = DateTime.Now.ToUnixTimeInt();
                            foreach (var item in redis_data.Values)
                            {
                                if (item != null) {
                                    // 일정 시간 지난건 제외
                                    if (update_time_out > (now_time - item.update_time)) {
                                        // gm observer는 gm observer , board 제외
                                        if (rpcinfo.type == wre_server_type.gmobserver) {
                                            if (item.type != wre_server_type.gmobserver)// && wre_server_type.board != item.type)
                                                list.Add(item);
                                        }
                                        // gm server는 gm observer 만
                                        //else if (rpcinfo.type == wre_server_type.gmserver) {
                                        //    if (item.type == wre_server_type.gmobserver)// || wre_server_type.gamemanager == item.type)
                                        //        list.Add(item);
                                        //}
                                    }
                                }
                            }
                        }

                        if (list.Count > 0) {
                            // gm server 는 gm observer 만 연결
                            //if (rpcinfo.type == wre_server_type.gmserver) {
                            //    for (int i = 0; i < list.Count; i++) {
                            //        PostConnect(list[i]);
                            //    }
                            //}
                            //// gm observer 는 gm server 만 연결
                            //else if(wre_server_type.gmobserver == rpcinfo.type)
                            //{
                            //    for (int i = 0; i < list.Count; i++)
                            //    {
                            //        if (wre_server_type.gmserver == list[i].type)
                            //        {
                            //            PostConnect(list[i]);
                            //            break;
                            //        }
                            //    }
                            //}

                            if (rpcinfo.type == wre_server_type.gmobserver) {
                                //var temp = list.Find(e => e.type == wre_server_type.board);
                                //if (temp != null)
                                //    list.Remove(temp);

                                if (_sendRedisRpcinfo != null) {
                                    _sendRedisRpcinfo(list);
                                }
                            }
                        }	
                    }
                    catch (Exception e)
                    {
                        logger.Error(e.ToString());
                    }                    				
                }
			});
        }

        // gm server, gm observer 제외한 나머지 서버 사용
        // 자신의 정보를 gm observer에 전송한다.
        public void RunUpdate(Config.rpc_ config, wrs_redis_serverinfo serverinfo)
        {
            int delay_msec_time = config.update_sec;
            //int update_time_out = 10;// config.update_timeout_sec;

            _acceptor.Start();

            _redis_rpcinfo_self = serverinfo.rpcinfo;
           
            var task = Task.Run(async () =>
            {

                var list = new List<wrs_redis_rpcinfo>();

                logger.Info(Newtonsoft.Json.JsonConvert.SerializeObject(_redis_rpcinfo_self, Newtonsoft.Json.Formatting.Indented));

                string self_fullTag = $"{define.SERVER_TAG_NAME[(int)_redis_rpcinfo_self.type]}_{serverinfo.rpcinfo.index}";

                var cs = new wrg_redis_serverinfo_set_cs();
                cs.info = serverinfo;

                logger.Info($"{_serverName} start.");

                while (_loop)
                {
                    await Task.Delay(TimeSpan.FromSeconds(delay_msec_time), _tokenSource.Token).ConfigureAwait(false);

                    if (!_loop)
                        break;

                    try
                    {

                        //if (null != _redis)
                        //{
                        //    // self rpc info update
                        //    {
                        //        _redis_rpcinfo_self.update_time = DateTime.Now.ToUnixTimeInt();
                        //        await _redis.SetHashDataStreamAsync(Redis.RedisRpcserver.KEY_RPC_INFO, self_fullTag, _redis_rpcinfo_self).ConfigureAwait(false);
                        //    }

                        //    list.Clear();

                        //    // get rpc servers info
                        //    var redis_data = await _redis.GetHashDataAllToDictionaryStreamAsync<wrs_redis_rpcinfo>(Redis.RedisRpcserver.KEY_RPC_INFO).ConfigureAwait(false);

                        //    if (null != redis_data)
                        //    {
                        //        int now_time = DateTime.Now.ToUnixTimeInt();

                        //        foreach (var item in redis_data.Values)
                        //        {
                        //            if (null != item)
                        //            {
                        //                // 일정 시간 지난건 제외
                        //                if (update_time_out > (now_time - item.update_time))
                        //                {
                        //                    // gm observer는 gm observer , board 제외
                        //                    if (wre_server_type.lobby == _redis_rpcinfo_self.type)
                        //                    {
                        //                        if (wre_server_type.lobby != item.type && wre_server_type.gmobserver != item.type && wre_server_type.board != item.type && wre_server_type.login != item.type)
                        //                        {
                        //                            list.Add(item);
                        //                        }
                        //                    }
                                           
                        //                }
                        //            }
                        //        }
                        //    }
                        //    if (0 < list.Count)
                        //    {
                              
                        //        {
                        //            for (int i = 0; i < list.Count; i++)
                        //            {

                        //                var tmprpc = GetRpc(list[i].type, list[i].index);

                        //                if (null != tmprpc)
                        //                {
                        //                    cs.info.gameserver_info.state = 0;

                        //                    tmprpc.clientToken.PostSend(cs);
                        //                }
                        //                else
                        //                {

                        //                    ConnectRpcServer(list[i].type, list[i]);
                        //                }
                        //               // PostConnect(list[i]);
                        //            }
                        //        }
                        //    }
                        //}
                        var rpc = GetRpc(wre_server_type.gmobserver, 0);

                        if (rpc != null) {
                            cs.info.gameserver_info.state = 0;

                            rpc.clientToken.Send(cs);
                        } else {
                            ConnectRpcServer(wre_server_type.gmobserver, null);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e.ToString());
                    }

                    ConnectRpcServer();
                }
            });
        }

        public void Stop()
        {
            _loop = false;

            _acceptor.Stop();
            _dicSession.Clear();
            if (_tokenSource != null) {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
            }
        }

        private bool PostConnect(wrs_redis_rpcinfo redis_info)
        {

            string rpc_fulltag = $"{define.SERVER_TAG_NAME[(int)redis_info.type]}_{redis_info.index}";

            int temp = 0;
            if (_dicConnectTry.TryGetValue(rpc_fulltag, out temp)) {
                logger.Info($"_dicConnectTry exist rpc : {rpc_fulltag}");
                return false;
            }

            if (GetRpc(redis_info.type, redis_info.index) == null) {
                //WCS.logger.Info($"try rpc connect, {rpc_fulltag}, {ip}, {redis_info.port}");
                          
                RpcToken token = new RpcToken(redis_info.type, redis_info.index, redis_info.ip, redis_info.port);

                _dicConnectTry.TryAdd(rpc_fulltag, temp);
                                
                if (!_connector.PostConnect(token)) {
                    token = null;

                    _dicConnectTry.Remove(rpc_fulltag, out temp);

                    return false;
                }                

                return true;
            } else {
               // WCS.logger.Info($"exist rpc connection : {rpc_fulltag}");
            }

            return false;
        }

        public void SetRedisRpcinfo(List<wrs_redis_rpcinfo> list)
        {
            _queue_connection_rpcinfo.Enqueue(list);            
        }

        public void ConnectRpcServer(wre_server_type server_type, wrs_redis_rpcinfo rpcinfo)
        {
            if (server_type == wre_server_type.gmobserver) {
                if (_redis_rpcinfo_observer == null && rpcinfo != null) {
                    _redis_rpcinfo_observer = rpcinfo;
                }

                if (_redis_rpcinfo_observer != null) {
                    PostConnect(_redis_rpcinfo_observer);
                }
            } else if (rpcinfo != null) {
                PostConnect(rpcinfo);
            }
        }

        private void ConnectRpcServer()
        {
            List<wrs_redis_rpcinfo> list = null;
            if (_queue_connection_rpcinfo.TryDequeue(out list))
            {
                try
                {
                    if (list != null && list.Count > 0) {
                        for (int i = 0; i < list.Count; i++) {
                            var rpcinfo = list[i];
                            if (rpcinfo.type == _redis_rpcinfo_self.type && rpcinfo.index == _redis_rpcinfo_self.index)
                                continue;
                            
                            PostConnect(rpcinfo);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.ToString());
                }

                list = null;
            }
        }

        public void Accepted(ClientToken token)
        {
            logger.Info($"Accepted Socket : {token.socket?.RemoteEndPoint}");

            _dicAcceptToken.TryAdd(token, token);

            if (0 == token.RpcSessionID)
            {
                token.RpcSessionID = WcatRandom.instance.Get();
                
            }
            ClientToken tempToken;
            if (!_dicSession.TryGetValue(token.RpcSessionID, out tempToken))
            {
                _dicSession.TryAdd(token.RpcSessionID, token);
            }
        }

        public void DisConnectedAccept(ClientToken token)
        {
            logger.Info($"DisConnectedAccept Socket : {token.socket?.RemoteEndPoint}");

            ClientToken temp = null;
            if (_dicAcceptToken.TryRemove(token, out temp)) {
                if (temp != null && _removeManagedServer != null) {
                    _removeManagedServer(token);
                }
            }
            if (token.RpcSessionID != 0)
            {
                ClientToken temptoken = null;
                _dicSession.TryRemove(token.RpcSessionID, out temptoken);
            }

        }

        public void Connected(ConnectToken rpcToken)
        {
            var token = rpcToken as RpcToken;

            string rpc_fulltag = $"{WCS.define.SERVER_TAG_NAME[(int)token.serverType]}_{token.index}";
            int temp = 0;
            if (!_dicConnectTry.Remove(rpc_fulltag, out temp)) {
                logger.Info($"_dicConnectTry.Remove() fail, {rpc_fulltag}");
            } else {
                logger.Info($"_dicConnectTry.Remove() success, {rpc_fulltag}");
            }

            logger.Info($"{rpc_fulltag}, {rpcToken.endpoint.ToString()}");

            if (0 == token.clientToken.RpcSessionID)
            {
                token.clientToken.RpcSessionID = WcatRandom.instance.Get();
            }
            
            
            _dicFullTag.TryAdd(rpc_fulltag, token);

            List<RpcToken> list;
            if (!_dicServerType.TryGetValue(token.serverType, out list)) {
                list = new List<RpcToken>();
                _dicServerType.TryAdd(token.serverType, list);
            }

            lock (_lock_dic_server_type) {
                list.Add(token);
            }

            if (_connectedCallback != null) {
                _connectedCallback(token.serverType, token.index, token.clientToken);
            }
            ClientToken tempToken;
            if (!_dicSession.TryGetValue(token.clientToken.RpcSessionID, out tempToken))
            {
                _dicSession.TryAdd(token.clientToken.RpcSessionID, token.clientToken);
            }
        }

        public void ConnectFail(ConnectToken rpcToken)
        {
            var token = rpcToken as RpcToken;

            string rpc_fulltag = $"{define.SERVER_TAG_NAME[(int)token.serverType]}_{token.index}";

            int temp = 0;
            _dicConnectTry.Remove(rpc_fulltag, out temp);

            //WCS.logger.Info($"{rpc_fulltag}");
        }

        public void DisConnected(ConnectToken rpcToken)
        {
            var token = rpcToken as RpcToken;

            string fulltag = $"{define.SERVER_TAG_NAME[(int)token.serverType]}_{token.index}";

            logger.Info($"{fulltag}, {rpcToken.endpoint.ToString()}");
           
            RpcToken temp;
            _dicFullTag.TryRemove(fulltag, out temp);

            List<RpcToken> tempToken;
            if (_dicServerType.TryGetValue(token.serverType, out tempToken)) {
                lock (_lock_dic_server_type) {
                    var temp2 = tempToken.Find(e => e.index == token.index);
                    if (temp2 != null) {
                        tempToken.Remove(temp2);
                    }
                }
            }
            if (token.clientToken.RpcSessionID != 0)
            {
                ClientToken temptoken = null;
                _dicSession.TryRemove(token.clientToken.RpcSessionID, out temptoken);
            }

        }

        public async Task SetRedisRpcinfo(wrs_redis_rpcinfo info)
        {
            await _redis.SetHashDataStreamAsync(Redis.RedisRpcserver.KEY_RPC_INFO, $"{define.SERVER_TAG_NAME[(int)info.type]}_{info.index}", info).ConfigureAwait(false);
        }


        private void SendsRedisRpcinfo(List<wrs_redis_rpcinfo> list)
        {
            var sc = new wrg_redis_rpcinfo_set_sc();
            sc.list = list;

            var stream = SendStreamPool.instance.CreateSendStream(sc);
            foreach (var item in _dicFullTag.Values)
            {
                if (item.serverType != wre_server_type.gmobserver /*&& item.serverType != wre_server_type.gmserver && item.serverType != wre_server_type.board*/) {
                    item.clientToken.PostSend(stream);
                }
            }

            SendStreamPool.instance.Push(stream);
        }

        public wrs_redis_rpcinfo GetRedisRpcinfo(wre_server_type type)
        {
            wrs_redis_rpcinfo rpcinfo = null;

            var rpc = GetRpc(type);
            if (rpc != null) {
                rpcinfo = new wrs_redis_rpcinfo();
                rpcinfo.type    = type;
                rpcinfo.index   = rpc.index;
                rpcinfo.ip      = rpc.ip;
                rpcinfo.port    = (ushort)rpc.port;
            }

            return rpcinfo;
        }

        public RpcToken GetRpc(wre_server_type type)
        {
            List<RpcToken> list;
            if (_dicServerType.TryGetValue(type, out list))
            {
                lock (_lock_dic_server_type)
                {
                    if (list.Count > 0)
                        return list[WcatRandom.instance.Get(list.Count)];
                }
            }

            return null;
        }

        public RpcToken GetRpc(wre_server_type type, int index)
        {
            RpcToken token = null;
            _dicFullTag.TryGetValue($"{define.SERVER_TAG_NAME[(int)type]}_{index}", out token);
            if (token == null) {
                logger.Error($"rpc server not found. {define.SERVER_TAG_NAME[(int)type]}_{index}");
            }

            return token;
        }

        public List<RpcToken> GetRpcAll(wre_server_type type)
        {
            List<RpcToken> list;
            if (_dicServerType.TryGetValue(type, out list)) {
                lock (_lock_dic_server_type)
                    return list.ToList();
            }

            return null;
        }

        public wrs_redis_rpcinfo MyRpcInfo()
        {
            return _redis_rpcinfo_self;
        }

        public ClientToken GetSession(long session_id)
        {
            ClientToken token = null;
            _dicSession.TryGetValue(session_id, out token);

            if (null == token)
            {
                WCS.logger.Error($"session not found. {session_id}");
            }

            return token;
        }
    }
}

#endif