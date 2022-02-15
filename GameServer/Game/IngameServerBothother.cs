using System.Collections.Generic;
//using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LetsBaseball;
using WCS;
using WCS.Network;

public partial class IngameServer
{
    /// <summary>
    /// both other : 나와 상대방에게 패킷 전송 : 클라패킷 받은 후 서버에서 나와 상대쪽 클라에 바로 전송 (양쪽 모두에게 바로전송)
    /// </summary>

    bool IsAbandon = false;     // 나의 기권여부

    #region request
    public void ReqGameSkill()
    {
        ClientToken session = clientToken;

        wcg_game_skill_cs cpacket = new wcg_game_skill_cs();
        cpacket.uuid = AuthServer.Inst.Uuid;
        if (GInfo.myTeam.turn == PlayTurn.Offense)
            cpacket.player_id = GInfo.myTeam.lineup.GetBatterID();
        else
            cpacket.player_id = GInfo.myTeam.lineup.GetPitcherID();

        logger.Info(Define.StrSB("++++++++++++ ReqGameSkill Send : turn : ", GInfo.myTeam.turn, " : id : ", cpacket.player_id));

        session.Send(cpacket);
    }

    public void ReqBallResult(wce_ball_result result, bool run1, bool run2, bool run3, int fence, int tick)
    {
        ClientToken session = clientToken;

        wcg_game_batter_result_cs cpacket = new wcg_game_batter_result_cs();
        cpacket.uuid = AuthServer.Inst.Uuid;
        cpacket.ball_type = result;
        cpacket.runner1 = run1;
        cpacket.runner2 = run2;
        cpacket.runner3 = run3;
        cpacket.fence = fence;
        cpacket.elpased_game_tick = tick;

        logger.Info(Define.StrSB("++++++++++++ ReqBallResult Send : result = ", result.ToString(), " runner1 = ", run1.ToString(),
            " runner2 = ", run2.ToString(), " runner3 = ", run3.ToString(), " fence = ", fence.ToString()));

        session.Send(cpacket);
    }

    public void ReqGameExit()
    {
        ClientToken session = clientToken;

        wcg_game_room_exit_cs cpacket = new wcg_game_room_exit_cs();

        if(GameManager.Inst.state == GameState.InGame)
            IsAbandon = true;   // 기권함..
        logger.Info("++++++++++++ ReqGameExit Send ! ");

        session.Send(cpacket);
    }
    #endregion

    #region response
    Queue<wcg_game_skill_sc> skill = new Queue<wcg_game_skill_sc>();
    public async UniTask RecvGameSkill(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_skill_sc();

        sc.Deserialize(readStream.br);        

        skill.Enqueue(sc);
        int skillcount = 2;
        if (IMgr.Inst._Connect.netState == IngameConnect.NetState.EnemyMissing) skillcount = 1; // 상대방이 실종인상태에선 한쪽만 받아도 진행
        if (skill.Count >= skillcount)   // 양쪽 모두 받음
        {
            Dispatcher.RunOnMainThread(async () =>
            {
                await UniTask.Yield();
                logger.Info(Define.StrSB("--------------mp_game_skill_sc : ", sc.result));

                // skill start
                wcs_skill_info[] hitSkill = null;
                wcs_skill_info[] pitSkill = null;
                while (skill.Count > 0)
                {
                    wcg_game_skill_sc info = skill.Dequeue();
                    if (info.skills != null)
                    {
                        for (int i = 0; i < info.skills.Count; i++)
                        {
                            if (info.skills[i].skill_id < 10000) info.result = 0; // 타자
                            else info.result = 1; // 투수
                        }
                        if (info.result == 0)
                            hitSkill = info.skills.ToArray();
                        else
                            pitSkill = info.skills.ToArray();
                    }
                }
                skill.Clear();
                wcs_skill_info[][] data = { pitSkill, hitSkill };
                IMgr.Inst._Flow.StartFlow(IngameFlow.Flow.StartSkill, data);
            });
        }
    }

    public async UniTask RecvBallResult(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_batter_result_sc();

        sc.Deserialize(readStream.br);

        Dispatcher.RunOnMainThread(async () =>
        {
            await UniTask.Yield();
            float delay = IMgr.Inst._Connect.latency = IMgr.Inst._Connect.CalLatency(sc.elpased_game_tick);
            logger.Info(Define.StrSB("--------------mp_game_batter_result_sc : ", sc.ball_type, " : delay = ", delay));

            await UniTask.Delay(System.TimeSpan.FromSeconds(delay));
            //this.After(delay, () =>
            //{
                //UnityEngine.Time.timeScale = 1f;
                if (sc.fence == -1) IMgr.Inst._BallMgr.ResetPhysics();
                IMgr.Inst._Flow.StartFlow(IngameFlow.Flow.ResultGame, sc.ball_type);
                IMgr.Inst._ScoreMgr.netInfo = sc;
            //});
        });
    }

    public async UniTask RecvGameExit(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_room_exit_sc();

        sc.Deserialize(readStream.br);

        Dispatcher.RunOnMainThread(async () =>
        {
            await UniTask.Yield();
            if (IMgr.Inst._InGameMgr.IsGameEnd) return;
            logger.Info(Define.StrSB("--------------mp_game_room_exit_sc : ", sc.uuid.ToString()));

            if (IMgr.IsCreate)
            {
                IMgr.Inst._InGameMgr.IsGameEnd = true;
                if (!IsAbandon)
                {
                    IMgr.Inst._InGameMgr.endDelayTime = 3f;
                    GMgr.Inst._CommonPopupMgr.CloseAllPopup();
                    if (GInfo.myTeam.userInfo.ID != sc.uuid)
                    {
                        // 상대방의 정상적이지 않는 종료 (기권, etc..)
                        GMgr.Inst._CommonPopupMgr.OpenPopup(CommonPopup.PopupState.BoxIngame, "help_system_2", IMgr.Inst._InGameMgr.endDelayTime);
                    }
                    else
                    {
                        // 정상적이지 못한 종료..
                        GInfo.myTeam.playResult = PlayResult.Lose;
                        GMgr.Inst._CommonPopupMgr.OpenPopup(CommonPopup.PopupState.BoxIngame, "help_system_1", IMgr.Inst._InGameMgr.endDelayTime);
                    }
                }
                IsAbandon = false;
            }
        });
    }
    #endregion
}
