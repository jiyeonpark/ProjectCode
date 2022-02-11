using System;
//using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LetsBaseball.Network.Http;
using WCS;
using WCS.Network;
using WCS.Network.Http;
using UnityEngine;

public partial class LobbyServer
{
    public MatchState matchState { get; private set; }
    public PoolingState pollingState = PoolingState.None;
    bool bMatchComplete = false;
    bool bMatchCancel = false;

    public Payloader<wcw_lobby_match_start_sc> SendMatchStart(long uid)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLogin, PacketUrl.URL_lobby_match_start);

        var request = new wcw_lobby_match_start_cs();
        request.uuid = uid;

        return http.Post<wcw_lobby_match_start_sc>(url, request).Callback(
            success: (response) =>
            {
                //IsCheckTime = false;
                matchState = MatchState.MatchStart;
                pollingState = PoolingState.Pooling;

                _UpdateMatchPolling();
            },
            fail: (err) =>
            {
                CustomLog.Error(WLogType.all, "SendMatchStart : wce_err : ", err);
                //switch (err)
                //{
                //    case wce_err.already_game_playing: OMgr.Inst._OutGameMgr.Multi_JoinRoom(); break;
                //}
            });
    }

    public Payloader<wcw_lobby_match_cancel_sc> SendMatchCancel(long uid)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLogin, PacketUrl.URL_lobby_match_cancel);

        var request = new wcw_lobby_match_cancel_cs();
        request.uuid = uid;

        return http.Post<wcw_lobby_match_cancel_sc>(url, request).Callback(
            success: (response) =>
            {
                matchState = MatchState.None;
                pollingState = PoolingState.None;

                bMatchComplete = false;
                bMatchCancel = false;

                //IsCheckTime = false;
            });
    }

    public Payloader<wcw_lobby_match_pooling_sc> SendMatchPolling(long uid)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLogin, PacketUrl.URL_lobby_match_pooling);

        var request = new wcw_lobby_match_pooling_cs();
        request.uuid = uid;

        return http.Post<wcw_lobby_match_pooling_sc>(url, request).Callback(
            success: (response) =>
            {
                if ((PoolingState)response.state == PoolingState.BattleStart)
                {
                    pollingState = (PoolingState)response.state;

                    BaseLogicHttp.urlGame = response.game_server.ip;
                    BaseLogicHttp.gamePort = response.game_server.port;
                    BaseLogicHttp.roomID = response.room_id;
                    BaseLogicHttp.matchID = response.match_id;
                    BaseLogicHttp.matchType = response.match_type;

                    CustomLog.Log(WLogType.debug, "*** StartNetwork : ip = ", BaseLogicHttp.urlGame.ToString() , " : port = ", BaseLogicHttp.gamePort.ToString());
                    IngameServer.Inst.StartNetwork(BaseLogicHttp.urlGame, (ushort)BaseLogicHttp.gamePort);
                }
            });
    }

    void _UpdateMatchPolling()
    {
        UniTask task = UniTask.RunOnThreadPool(async () =>
        {

            if (matchState != MatchState.None)
            {
                bool sendcompleted = true;
                while (pollingState == PoolingState.Pooling)
                {
                    if (sendcompleted)
                    {
                        sendcompleted = false;
                        SendMatchPolling(AuthServer.Inst.Uuid).Callback(
                            success: (response) =>
                            {
                                if (pollingState == PoolingState.Pooling)
                                    sendcompleted = true;
                            });
                    }

                    if (pollingState != PoolingState.Pooling)
                        break;

                    //await Task.Delay(TimeSpan.FromSeconds(1.0f), IngameServer.Inst._tokenSource.Token).ConfigureAwait(false);
                    await UniTask.Delay(TimeSpan.FromSeconds(1.0f), false, PlayerLoopTiming.Update, IngameServer.Inst._tokenSource.Token);
                }

                if (pollingState == PoolingState.BattleStart)
                {
                    //TODO: PvP로 연결
                    //IsCheckTime = true;
                }


                else if (pollingState == PoolingState.MatchCancel)
                {
                    //TODO: Match Cancel인 경우 RoomResult를 전송하지 않는 방식으로 로비로 돌리는 기능이 필요...
                    //WaitForSeconds 때문에 Cancel을 늦게 받을 수 있음?
                    //NetworkManager.Instance.LastKeepAlive = -1f;
                }

                WCS.logger.Info(Define.StrSB(" m_pollingState : ", pollingState.ToString(), " Polling End"));
            }
            OnMatchClear();
        }, false);

    }

    void OnMatchClear()
    {

        if (!bMatchComplete && !bMatchCancel)
        {
            WCS.logger.Info(Define.StrSB("m_bMatchComplete:", bMatchComplete, " m_bMatchCancel:", bMatchCancel));
            //XPopupCommon.sCreateWithTextKey(null, "UI_Common_PopupTitle_Notice", "SEARCH_MATCH_USER_ERROR");
            // "상대를 찾지 못했습니다"로 나왔지만 매칭중 홈으로가서 폴링을 못보내서 자연스레 MatchCancel이 온것이므로 "매칭 취소"가 맞음.
            // XPopupCommon.sCreateWithTextKey(null, "UI_Common_PopupTitle_Notice", "UI_BattleMatch_PopupText_CancelMatch");
        }

        matchState = MatchState.None;
        pollingState = PoolingState.None;

        bMatchComplete = false;
        bMatchCancel = false;
    }

    // 매칭이 정상적으로 됬는지 체크 : 이미 매칭이 됬는데 다음 스텝으로 안넘어가는경우 처리..
    //bool IsCheckTime = false;
    //float checkTime = 0f;
    //void UpdateCheckMatching()
    //{
    //    if (IsCheckTime)
    //    {
    //        checkTime += Time.deltaTime;
    //        if (checkTime > 10f)
    //        {
    //            IsCheckTime = false;
    //            checkTime = 0f;
    //            OMgr.Inst._OutGameMgr.Multi_JoinRoom();
    //        }
    //    }
    //    else checkTime = 0f;        
    //}
}
