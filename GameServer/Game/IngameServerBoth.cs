//using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LetsBaseball;
using WCS;
using WCS.Network;

public partial class IngameServer
{
    /// <summary>
    /// both : 양쪽 모두 패킷 전송 : 양쪽 클라패킷 받은 후 서버에서 양쪽 클라에 동시에 전송
    /// </summary>

    #region request
    public void ReqChangeTrun()
    {
        ClientToken session = clientToken;

        wcg_game_change_trun_cs cpacket = new wcg_game_change_trun_cs();
        cpacket.uuid = AuthServer.Inst.Uuid;
        // 아직 IngameFlow의 changeTrun 로직을 실행전 패킷요청이라서 현재 turn이 아닌 다음 turn 상태를 보내야한다..
        cpacket.attack_side = (short)(IMgr.Inst._ScoreMgr.gameInfo.turn == TeamPosition.Away ? TeamPosition.Home : TeamPosition.Away);

        logger.Info(Define.StrSB("++++++++++++ ReqChangeTrun Send ! : ", IMgr.Inst._ScoreMgr.gameInfo.turn.ToString(), " : ", cpacket.attack_side.ToString()));

        session.Send(cpacket);
    }

    public void ReqChangeView(bool changeplayer, int tick)
    {
        ClientToken session = clientToken;

        wcg_game_change_view_cs cpacket = new wcg_game_change_view_cs();
        cpacket.uuid = AuthServer.Inst.Uuid;
        cpacket.attack_side = (short)IMgr.Inst._ScoreMgr.gameInfo.turn;
        cpacket.pitcher_id = GInfo.Defence.GetTmPlayerInfo(TeamPlayerPos.Pitcher).Index;
        cpacket.batter_id = GInfo.Offence.lineup.GetNextBatterID();
        cpacket.batter_change = changeplayer;
        cpacket.current_game_tick = tick;

        logger.Info(Define.StrSB("++++++++++++ ReqChangeView Send ! : ", IMgr.Inst._ScoreMgr.gameInfo.turn.ToString(), " : ", cpacket.attack_side.ToString()));

        session.Send(cpacket);
    }
    #endregion

    #region response
    public async UniTask RecvGameStart(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_start_sc();

        sc.Deserialize(readStream.br);

        // 로딩완료 게임시작..
        Dispatcher.RunOnMainThread(async () =>
        {
            await UniTask.Yield();
            logger.Info(Define.StrSB("--------------mp_game_start_sc : ", sc.start_time.ToString()));

            IsGameStart = true;
            GameEvent.OnLoadCompletedEvent();
        });
    }

    public async UniTask RecvChangeTrun(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_change_trun_sc();

        sc.Deserialize(readStream.br);

        Dispatcher.RunOnMainThread(async () =>
        {
            await UniTask.Yield();
            logger.Info(Define.StrSB("--------------mp_game_change_trun_sc : ", sc.current_game_tick.ToString()));

            IMgr.Inst._Connect.ResetTick();
            IMgr.Inst._Flow.StartFlow(IngameFlow.Flow.ChangeTurn);
        });
    }

    public async UniTask RecvChangeView(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_change_view_sc();

        sc.Deserialize(readStream.br);

        Dispatcher.RunOnMainThread(async () =>
        {
            await UniTask.Yield();

            IMgr.Inst._Connect.latency = sc.sync_time;
            if (GInfo.myTeam.turn == PlayTurn.Defence) IMgr.Inst._Connect.latency += IMgr.Inst._Connect.FixLatency;
            logger.Info(Define.StrSB("--------------mp_game_change_view_sc sync_time : ", sc.sync_time, " : latency : ", IMgr.Inst._Connect.latency));
            
            if (sc.fences == null)
            {
                logger.Info("sc.fences == null");
                IMgr.Inst._Flow.StartFlow(IngameFlow.Flow.ChangeView, null);
            }
            else
                IMgr.Inst._Flow.StartFlow(IngameFlow.Flow.ChangeView, sc.fences.ToArray());
        });
    }
    #endregion
}
