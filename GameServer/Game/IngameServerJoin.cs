//using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LetsBaseball;
using LetsBaseball.Network.Http;
using WCS;
using WCS.Network;

public partial class IngameServer
{
    /// <summary>
    /// both  : 양쪽 모두 패킷 전송 : 양쪽 클라패킷 받은 후 서버에서 양쪽 클라에 동시에 전송
    /// </summary>


    private ClientToken clientToken = null;

    #region request
    public void ReqLogin(ClientToken session = null)
    {
        if (clientToken != null && session == null) session = clientToken;
        else clientToken = session;

        wcg_game_login_cs cpacket = new wcg_game_login_cs();
        cpacket.uuid = AuthServer.Inst.Uuid;
        cpacket.access_token = AuthServer.Inst.AccessToken;
        cpacket.room_id = BaseLogicHttp.roomID;
        cpacket.match_id = BaseLogicHttp.matchID;
        cpacket.match_type = BaseLogicHttp.matchType;

        logger.Info("++++++++++++ ReqLogin Send !");

        session.Send(cpacket);
    }

    public void ReqLoadComplete(ClientToken session = null)
    {
        if (clientToken != null && session == null) session = clientToken;
        else clientToken = session;

        // 맵 & 모델 로딩 완료 패킷
        wcg_game_load_complate_cs cpacket = new wcg_game_load_complate_cs();
        cpacket.uuid = AuthServer.Inst.Uuid;
        cpacket.game_uid = BaseLogicHttp.roomID;

        logger.Info(Define.StrSB("++++++++++++ ReqLoadComplete Send ! : ", cpacket.game_uid.ToString()));
        
        IsGameStart = false;
        this.After(10f, () =>
        {
            if (IsGameStart == false)
            {
                logger.Info("--- GameStart time over 10s ---");
                GMgr.Inst._CommonPopupMgr.OpenPopupTxt(CommonPopup.PopupState.BoxNone, "Error : 로그확인. \n 로비로 이동합니다.", (type) =>
                {
                    IMgr.Inst._InGameMgr.IsGameEnd = true;
                    IMgr.Inst._InGameMgr.endDelayTime = 0f;
                    IMgr.Inst._Connect.CallGameEnd();
                });
            }
        });

        session.Send(cpacket);
    }

    public void ReqBackground(int background)
    {
        ClientToken session = clientToken;

        wcg_game_ground_cs cpacket = new wcg_game_ground_cs();
        cpacket.background = background;

        logger.Info(Define.StrSB("++++++++++++ ReqBackground Send : ", background));

        session.Send(cpacket);
    }

    public void ReqReconnect(ClientToken session = null)
    {
        if (clientToken != null && session == null) session = clientToken;
        else clientToken = session;

        wcg_game_reconnect_cs cpacket = new wcg_game_reconnect_cs();

        logger.Info("++++++++++++ ReqReconnect Send ");

        session.Send(cpacket);
    }
    #endregion

    #region response
    public async UniTask RecvLogin(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_login_sc();

        sc.Deserialize(readStream.br);

        logger.Info(Define.StrSB("-------------- game_login_sc : ", sc.result.ToString()));
    }
    public async UniTask RecvLoginComplete(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_login_complate_sc();
        sc.Deserialize(readStream.br);

        logger.Info(Define.StrSB("-------------- game_login_complate_sc : stadium = ", sc.stadium.ToString(), " : home = ", sc.home.nickname, " : away = ", sc.away.nickname));

        // 맵 & 모델 로드..
        Dispatcher.RunOnMainThread(async () =>
        {
            await UniTask.Yield();
            IsLoginComplate = true;
            OMgr.Inst._OutGameMgr.Single_MultiRoom(sc.home, sc.away);
        });
    }


    public async UniTask RecvLoadComplete(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_load_complate_sc();
        sc.Deserialize(readStream.br);

        logger.Info(Define.StrSB("-------------- game_load_complate_sc : ", sc.result.ToString()));
    }

    public async UniTask RecvBackground(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_ground_sc();
        sc.Deserialize(readStream.br);

        Dispatcher.RunOnMainThread(async () =>
        {
            await UniTask.Yield();
            logger.Info(Define.StrSB("-------------- game_ground_sc : ", sc.background.ToString()));

            if (sc.background == 1)
            {
                GMgr.Inst._SystemMgr.SetLocalTimeScale(0f);
                GMgr.Inst._CommonPopupMgr.CloseAllPopup();
                GMgr.Inst._CommonPopupMgr.OpenPopup(CommonPopup.PopupState.Waiting_NoBlock);
            }
            else
            {
                GMgr.Inst._SystemMgr.SetLocalTimeScale(1f);
                GMgr.Inst._CommonPopupMgr.ClosePopup();
                IMgr.Inst._Connect.CallDefenderPos(true);
            }
        });
    }

    public async UniTask RecvReconnect(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_reconnect_sc();
        sc.Deserialize(readStream.br);

        Dispatcher.RunOnMainThread(async () =>
        {
            await UniTask.Yield();
            logger.Info(Define.StrSB("--------------!!! game_reconnect_sc : current_batter= ", sc.current_batter.ToString(),
                 ": 1run = ", sc.runners[0].ToString(), ": 2run = ", sc.runners[1].ToString(), ": 3run = ", sc.runners[2].ToString()));

            GMgr.Inst._CommonPopupMgr.CloseAllPopup();
            GMgr.Inst._SystemMgr.SetLocalTimeScale(1f);

            IMgr.Inst._Connect.NetRePing();
            IMgr.Inst._ScoreMgr.NetInfoSetting(sc);
            if ((int)GInfo.myTeam.teamPos == sc.currnet_attack_side)
                IMgr.Inst._InGameMgr.SetTurnType(PlayTurn.Offense);
            else
                IMgr.Inst._InGameMgr.SetTurnType(PlayTurn.Defence);
            if (sc.current_batter == 0)
            {
                GInfo.Offence.lineup.currentIdx--;
                if (GInfo.Offence.lineup.currentIdx < 0) GInfo.Offence.lineup.currentIdx = 0;
            }
            else
            {
                for (TeamPlayerPos pos = TeamPlayerPos.Hitter; pos < TeamPlayerPos.Max; pos++)
                {
                    if (GInfo.Offence.lineup.playerID[pos] == sc.current_batter)
                    {
                        // 이전타순(idx) 계산
                        int idx = pos - TeamPlayerPos.Hitter - 1;
                        if (idx < 0) idx = TeamPlayerPos.Max - TeamPlayerPos.Hitter - 1;    // 맨 뒤순서
                        GInfo.Offence.lineup.currentIdx = idx;
                        break;
                    }
                }
            }
            IMgr.Inst._ResultFlow.changePlayer = true;
            if (sc.runners != null)
            {
                for (int i = 0; i < sc.runners.Count; i++)
                    IMgr.Inst._RunnerMgr.runnerInfo[i + 1] = sc.runners[i];
            }
            IMgr.Inst._CameraMgr.cameraEvent.OnChangeTurn(true);
            if (IMgr.Inst._InGameMgr.turn == PlayTurn.Defence)
                IMgr.Inst._StadiumMgr.stadiumlist.OnChangeTurn(false);
            else
                IMgr.Inst._StadiumMgr.stadiumlist.OnChangeTurn(true);
            IMgr.Inst._Flow.StartFlow(IngameFlow.Flow.ChangeTurn, true);
        });
    }

    public async UniTask RecvMatchRoomExit(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_matchroom_exit_sc();
        sc.Deserialize(readStream.br);

        logger.Info("-------------- game_matchroom_exit_sc ");
    }
    #endregion
}
