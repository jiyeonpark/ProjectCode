using System;
using System.Threading;
//using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using WCS;
using WCS.Network;
using LetsBaseball;
using LetsBaseball.Network.Http;

public partial class IngameServer
{
    public CancellationTokenSource _tokenSource = new CancellationTokenSource();

    private ClientNetwork _clientNetwork;
    private bool _task_loop = true;
    public string board_server_url { get; set; }
    private ClientNetwork.SessionEventType type = ClientNetwork.SessionEventType.DisConnected;
    public bool IsConnected { get { return type == ClientNetwork.SessionEventType.Connected ? true : false; } }
    private bool IsReconnect = false;
    private bool IsLoginComplate = false;
    private bool IsGameStart = false;

    private void UpdateGamePacket()
    {
        UniTask task = UniTask.RunOnThreadPool(async () =>
        {
            while (_task_loop)
            {
                _clientNetwork.PacketEvent.WaitOne();

                if (false == _task_loop)
                    break;

                netContext.Context context = null;

                while (_clientNetwork.GetContext(out context))
                {
                    //await GamePacketProcess(context.session, context.stream).ConfigureAwait(false);
                    await GamePacketProcess(context.token, context.stream);

                    
                    //ReadStreamPool.instance.Push(context.stream);
                    _clientNetwork.PushContext(context);
                }

                _clientNetwork.PacketEvent.Reset();
            }
        }, false);
    }

    public void StartNetwork(string server_ip, ushort port, bool reconnet = false)
    {
        IsLoginComplate = false;
        IsReconnect = reconnet;
        if (reconnet == false)
        {
            _clientNetwork = null;
            _clientNetwork = new ClientNetwork();
            _clientNetwork.SessionEventCallback += SessionEventCallback;
            _clientNetwork.SessionOpenCallback += SessionOpenCallback;
            _clientNetwork.SessionCloseCallback += SessionCloseCallback;
            _clientNetwork.Initialize("dummy", 1, 16, 2);

            _clientNetwork.Connect(server_ip, port);
        }
        else
        {
            _clientNetwork.ReConnect(server_ip, port);
        }        
    }

    public void StopNetwork(bool closeSession)
    {
        if (closeSession)
            _task_loop = false;

        if (null != _clientNetwork)
        {
            _clientNetwork.DisconnetNetwork();
            //_clientNetwork = null;
        }

        if (closeSession)
        {
        }

        type = ClientNetwork.SessionEventType.DisConnected;
    }

    public void DisconnectNetwork()
    {
        logger.Info("Call DisconnectNetwork !!!");
        StopNetwork(true);
    }

    public void Shutdown()
    {
        //_network.shutdown();
        // _gameView.Close();
        DisconnectNetwork();
        if (null != _tokenSource)
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }
    }

    // 재연결..

    public async UniTask Reconnect()
    {
        await UniTask.Yield();

        GMgr.Inst._CommonPopupMgr.CloseAllPopup();
        GMgr.Inst._CommonPopupMgr.OpenPopup(CommonPopup.PopupState.NetWait, "error_message_73");

        logger.Info(Define.StrSB("Call Reconnect start !!! [IsGameStart : ", IMgr.Inst._InGameMgr.IsGameStart,
            "] [IsGameEnd : ", IMgr.Inst._InGameMgr.IsGameEnd, 
            "] [IsConnected : ", IsConnected,
            "] [curNetReachability : ", GMgr.Inst._SystemMgr.curNetReachability, "]"));

        if (IMgr.IsCreate && IMgr.Inst._InGameMgr.IsGameStart && IMgr.Inst._InGameMgr.IsGameEnd == false)
        {
            //await UniTask.DelayFrame(5, PlayerLoopTiming.Update);

            int count = 0;
            float checktime = 3f;
            float timeout = 30f;
            while (IsConnected == false)
            {
                logger.Info(Define.StrSB("Reconnect time : ", timeout, " [curNetReachability : ", GMgr.Inst._SystemMgr.curNetReachability, "]"));

                if (GMgr.Inst._SystemMgr.curNetReachability == UnityEngine.NetworkReachability.NotReachable)
                {
                    if (timeout <= 0) break;
                    await UniTask.Delay(System.TimeSpan.FromSeconds(1f));
                    timeout -= 1f;
                }
                else
                {
                    if (timeout >= 0)
                    {
                        if (IsConnected) break;
                        if (count == 0)
                        {
                            logger.Info(Define.StrSB("Reconnect !!! == ", count.ToString()));
                            StartNetwork(BaseLogicHttp.urlGame, (ushort)BaseLogicHttp.gamePort, true);
                            count++;
                        }
                        else
                        {
                            if (type == ClientNetwork.SessionEventType.ConnectedFailed)
                            {
                                logger.Info(Define.StrSB("Reconnect !!! == ", count.ToString()));
                                StartNetwork(BaseLogicHttp.urlGame, (ushort)BaseLogicHttp.gamePort, true);
                                count++;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }

                    await UniTask.Delay(System.TimeSpan.FromSeconds(checktime));
                    timeout -= checktime;
                }
            }

            if (IsConnected == false)
            {
                Dispatcher.RunOnMainThread(async () =>
                {
                    await UniTask.Yield();
                    GMgr.Inst._CommonPopupMgr.ClosePopup();
                    IMgr.Inst._Connect.NetPopup();
                });
            }

            // 재연결 여부 체크 (30초)
            //await UniTask.Delay(System.TimeSpan.FromSeconds(30f));
            //if (IsConnected == false)
            //{
            //    Dispatcher.RunOnMainThread(async () =>
            //    {
            //        await UniTask.Yield();
            //        IMgr.Inst._Connect.NetPopup();
            //    });
            //}
        }
    }
    public void SessionOpenCallback(ClientToken token, bool reconnect)
    {
        //if (IsReconnect)
        //{
        //    ReqReconnect(token);
        //}
        //else
        //{
        //    ReqLogin(token);
        //}
    }

    public void SessionCloseCallback(long Session, wce_err err)
    {
        Dispatcher.RunOnMainThread(async () =>
        {
            await UniTask.Yield();
            logger.Info(Define.StrSB("SessionCloseCallback : Session = ", Session.ToString(), " : err = ", err.ToString()));

            // 정상적이지 못한 종료..
            GInfo.myTeam.playResult = PlayResult.Lose;
            IMgr.Inst._InGameMgr.endDelayTime = 3f;
            GMgr.Inst._CommonPopupMgr.CloseAllPopup();
            GMgr.Inst._CommonPopupMgr.OpenPopup(CommonPopup.PopupState.BoxIngame, "help_system_1", IMgr.Inst._InGameMgr.endDelayTime);
        });
    }
    public void SessionEventCallback(ClientNetwork.SessionEventType eventType, ClientToken token)
    {
        type = eventType;
        logger.Info(Define.StrSB("SessionEventCallback : ", eventType.ToString()));
        if (ClientNetwork.SessionEventType.Connected != eventType)
        {
            if (ClientNetwork.SessionEventType.DisConnected == eventType)
            {
                //if (OMgr.IsCreate && OMgr.Inst._OutGameMgr.matchType != OutGameManager.MatchType.first)
                //    OMgr.Inst._OutGameMgr.Multi_JoinRoom();
                if(IMgr.Inst._Connect.IsCallReconnect && IMgr.Inst._InGameMgr.IsGameEnd == false) 
                    Reconnect();
            }
            return;
        }

        // packet parser update 동작 시작
        _task_loop = true;
        UpdateGamePacket();


        Thread.Sleep(500);

        clientToken = token;

        if (IsReconnect)
        {            
            ReqReconnect(token);
        }
        else
        {
            ReqLogin(token);
            /*
            IsLoginComplate = false;
            Dispatcher.RunOnMainThread(async () =>
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(10f));
                //this.After(10f, () =>
                //{
                    if (IsLoginComplate == false)
                    {
                        logger.Info("--- LoginComplate time over 10s ---");
                        OMgr.Inst._OutGameMgr.Multi_JoinRoom();
                    }
                //});
            });
            */
        }
    }

    public void SendGameMessage<T>(T packet) where T : class, ISerialize
    {
        _clientNetwork.SendMessage(packet);
    }

    public void SendGameMessage<T>(T packet, Action dataRefesh) where T : class, ISerialize
    {
        // this.DataRefesh = dataRefesh;
        _clientNetwork.SendMessage(packet);
    }
}
