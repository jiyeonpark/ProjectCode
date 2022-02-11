//using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LetsBaseball;
using WCS;
using WCS.Network;

public partial class IngameServer
{
    /// <summary>
    /// mine : 서버에 내가 보내고 나만 받기..
    /// </summary>

    #region request
    public void ReqGameResult(wce_game_result result, wce_match_type matchType = wce_match_type.user)
    {
        ClientToken session = clientToken;

        wcg_game_result_cs cpacket = new wcg_game_result_cs();
        cpacket.uuid = AuthServer.Inst.Uuid;
        cpacket.result = result;
        cpacket.room_id = LetsBaseball.Network.Http.BaseLogicHttp.roomID;
        cpacket.match_id = LetsBaseball.Network.Http.BaseLogicHttp.matchID;
        cpacket.match_type = matchType;

        logger.Info("++++++++++++ ReqGameResult Send !");

        session.Send(cpacket);
    }

    public void ReqGamePing(int count, int tick, int interval)
    {
        ClientToken session = clientToken;

        wcg_game_ping_cs cpacket = new wcg_game_ping_cs();
        cpacket.send_count = count;
        cpacket.time_stamp = tick;
        cpacket.interval = interval;

        //logger.Info("++++++++++++ ReqGamePing Send !");

        session.Send(cpacket);
    }
    #endregion

    #region response
    public async UniTask RecvError(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_error_message_sc();

        sc.Deserialize(readStream.br);

        Dispatcher.RunOnMainThread(async () =>
        {
            await UniTask.Yield();
            logger.Info(Define.StrSB("--------------mp_error_message_sc : ", sc.error));

            IMgr.Inst._InGameMgr.IsGameEnd = true;
            GInfo.myTeam.playResult = PlayResult.Lose;
            GMgr.Inst._CommonPopupMgr.CloseAllPopup();
            GMgr.Inst._CommonPopupMgr.OpenPopup(CommonPopup.PopupState.BoxIngame, Define.StrSB("ERROR : \n", sc.error.ToString()));
        });
    }

    public async UniTask RecvGameResult(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_result_sc();

        sc.Deserialize(readStream.br);

        Dispatcher.RunOnMainThread(async () =>
        {
            await UniTask.Yield();
            logger.Info(Define.StrSB("--------------mp_game_result_sc : ", sc.result, " : ", sc.type.ToString()));

            IMgr.Inst._Connect.StopPingPong();
            GMgr.Inst._SystemMgr.SetLocalTimeScale(1f);
            GInfo.myTeam.playResult = (PlayResult)sc.result;
            GInfo.myTeam.serverResult = sc;

            if (sc.type == wce_game_result_type.abandon)
            {
                // 기권게임
                if (GInfo.myTeam.playResult == PlayResult.Win)
                {
                    // 상대방의 정상적이지 않는 종료 (기권, etc..)
                    IMgr.Inst._InGameMgr.endDelayTime = 3f;
                    GMgr.Inst._CommonPopupMgr.OpenPopup(CommonPopup.PopupState.BoxIngame, "help_system_2", IMgr.Inst._InGameMgr.endDelayTime);
                }
            }

            //my
            if (GInfo.myTeam.teamPos == TeamPosition.Home)
                GInfo.myTeam.score = IMgr.Inst._ScoreMgr.gameInfo.totalScoreH;
            else
                GInfo.myTeam.score = IMgr.Inst._ScoreMgr.gameInfo.totalScoreA;
            //enemy
            if (GInfo.enemyTeam.teamPos == TeamPosition.Home)
                GInfo.enemyTeam.score = IMgr.Inst._ScoreMgr.gameInfo.totalScoreH;
            else
                GInfo.enemyTeam.score = IMgr.Inst._ScoreMgr.gameInfo.totalScoreA;
            if (sc.capsule != null && sc.capsule.tid == 0) sc.capsule = null;   // tid 가 0이면 캡슐없음으로 간주 (데이터테이블에 없음)
            if (sc.shop_item != null) ServerInfo.shop.AddShopInfo(sc.shop_item);
            ServerInfo.capsule.AddServerCapsule(sc.capsule);
            ServerInfo.SetProperty(sc.trophypass_max_count);
            ServerInfo.user.SetCurrencies(sc.currencies);
            IMgr.Inst._Flow.StartFlow(IngameFlow.Flow.EndGame, GInfo.myTeam.playResult);
        });
    }

    public async UniTask RecvGamePing(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_ping_sc();

        sc.Deserialize(readStream.br);

        Dispatcher.RunOnMainThread(async () =>
        {
            await UniTask.Yield();
            //logger.Info("--------------game_ping_sc : ");

            IMgr.Inst._Connect.NetPong(sc.send_count, sc.time_stamp);
        });
    }
    #endregion
}
