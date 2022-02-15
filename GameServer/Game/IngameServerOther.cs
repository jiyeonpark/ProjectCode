using System.Collections.Generic;
//using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LetsBaseball;
using WCS;
using WCS.Network;

public partial class IngameServer
{
    /// <summary>
    /// other : 상대방에게 패킷 전송 : 클라패킷 받은 후 서버에서 상대쪽 클라에 바로 전송 (나에겐 오지않음, 상대방패킷만 받음)
    /// </summary>

    #region request
    public void ReqPitPos(float xpos)
    {
        ClientToken session = clientToken;

        wcg_game_pitching_pos_cs cpacket = new wcg_game_pitching_pos_cs();
        cpacket.uuid = AuthServer.Inst.Uuid;
        cpacket.x_pos = xpos;

        //logger.Info(Define.StrSB("++++++++++++ ReqPitPos Send : ", xpos));

        session.Send(cpacket);
    }

    public void ReqHitPos(float xpos, byte batterbox)
    {
        ClientToken session = clientToken;

        wcg_game_batter_pos_cs cpacket = new wcg_game_batter_pos_cs();
        cpacket.uuid = AuthServer.Inst.Uuid;
        cpacket.x_pos = xpos;
        cpacket.batterbox = batterbox;

        //logger.Info(Define.StrSB("++++++++++++ ReqHitPos Send : ", xpos));

        session.Send(cpacket);
    }

    public void ReqPitInput(float rate, float spin, bool pitching, byte state, int tick)
    {
        ClientToken session = clientToken;

        wcg_game_pitching_input_cs cpacket = new wcg_game_pitching_input_cs();
        cpacket.uuid = AuthServer.Inst.Uuid;
        cpacket.rate = rate;
        cpacket.spin = spin;
        cpacket.pitching_type = 0;  // 서버 작업끝나면 없애자.. (필요없음)
        cpacket.is_pitching = pitching;
        cpacket.state = state;
        cpacket.elpased_game_tick = tick;

        if(pitching) logger.Info(Define.StrSB("++++++++++++ ReqPitInput Send ! : ", pitching));

        session.Send(cpacket);
    }

    public void ReqSwingInput(int tick)
    {
        ClientToken session = clientToken;

        wcg_game_batter_input_cs cpacket = new wcg_game_batter_input_cs();
        cpacket.uuid = AuthServer.Inst.Uuid;
        cpacket.elpased_game_tick = tick;

        logger.Info("++++++++++++ ReqSwingInput Send !");

        session.Send(cpacket);
    }

    public void ReqBallUpdate(float power, UnityEngine.Vector3 pos, UnityEngine.Vector3 vec, byte type, int tick)
    {
        ClientToken session = clientToken;

        wcg_game_hit_ball_update_cs cpacket = new wcg_game_hit_ball_update_cs();
        cpacket.uuid = AuthServer.Inst.Uuid;
        cpacket.power = power;
        cpacket.pos_x = pos.x;
        cpacket.pos_y = pos.y;
        cpacket.pos_z = pos.z;
        cpacket.vec_x = vec.x;
        cpacket.vec_y = vec.y;
        cpacket.vec_z = vec.z;
        cpacket.type = type;
        cpacket.elpased_game_tick = tick;

        logger.Info(Define.StrSB("++++++++++++ ReqBallUpdate Send ! ", type));

        session.Send(cpacket);
    }

    public void ReqLatency(int time)
    {
        ClientToken session = clientToken;

        wcg_game_latency_cs cpacket = new wcg_game_latency_cs();
        cpacket.time_stamp = time;

        //logger.Info(Define.StrSB("++++++++++++ ReqLatency Send ! ", time));

        session.Send(cpacket);
    }

    public void ReqChat(short message)
    {
        ClientToken session = clientToken;

        wcg_game_chat_cs cpacket = new wcg_game_chat_cs();
        cpacket.uuid = AuthServer.Inst.Uuid;
        cpacket.message = message;

        logger.Info(Define.StrSB("++++++++++++ ReqChat Send ! ", message));

        session.Send(cpacket);
    }

    public void ReqDefenceSync(List<float> values, int tick)
    {
        ClientToken session = clientToken;

        wcg_game_defence_sync_cs cpacket = new wcg_game_defence_sync_cs();
        cpacket.values = values;
        cpacket.current_game_tick = tick;

        logger.Info("++++++++++++ ReqDefenceSync Send ! ");

        session.Send(cpacket);
    }
    #endregion

    #region response
    public async UniTask RecvPitPos(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_pitching_pos_sc();

        sc.Deserialize(readStream.br);

        Dispatcher.RunOnMainThread(async () =>
        {
            await UniTask.Yield();
            //logger.Info(Define.StrSB("--------------mp_game_pitching_pos_sc : ", sc.x_pos.ToString()));

            IMgr.Inst._InGameMgr.pitching.RpcPosX(sc.x_pos);
        });
    }

    public async UniTask RecvHitPos(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_batter_pos_sc();

        sc.Deserialize(readStream.br);

        Dispatcher.RunOnMainThread(async () =>
        {
            await UniTask.Yield();
            //logger.Info(Define.StrSB("--------------mp_game_batter_pos_sc : ", sc.x_pos.ToString()));

            IMgr.Inst._InGameMgr.swing.RpcPosX(sc.x_pos);
            //IMgr.Inst._InGameMgr.swing.SettingBatterBox((Swing.BatterBox)sc.batterbox);
        });
    }

    public async UniTask RecvPitInput(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_pitching_input_sc();

        sc.Deserialize(readStream.br);

        Dispatcher.RunOnMainThread(async () =>
        {
            await UniTask.Yield();
            float delay = IMgr.Inst._Connect.latency = IMgr.Inst._Connect.CalLatency(sc.elpased_game_tick);
            
            if (sc.is_pitching)
            {
                logger.Info(Define.StrSB("--------------mp_game_pitching_input_sc : ", ((BallState)sc.state).ToString(), " : delay = ", delay.ToString()));

                //this.After(delay, () => { IMgr.Inst._InGameMgr.pitching.RpcPitching(sc.rate, sc.spin); });
                await UniTask.Delay(System.TimeSpan.FromSeconds(delay));
                IMgr.Inst._InGameMgr.pitching.RpcPitching(sc.rate, sc.spin);
            }
            else
            {
                if ((BallState)sc.state == BallState.PitchReady)
                {
                    //this.After(delay, () => { IMgr.Inst._InGameMgr.pitching.RpcPitAni(true, sc.rate); });
                    await UniTask.Delay(System.TimeSpan.FromSeconds(delay));
                    IMgr.Inst._InGameMgr.pitching.RpcPitAni(true, sc.rate);
                }
                else
                    IMgr.Inst._InGameMgr.pitching.RpcPitAni(false, sc.rate);
            }
        });
    }

    public async UniTask RecvSwingInput(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_batter_input_sc();

        sc.Deserialize(readStream.br);

        Dispatcher.RunOnMainThread(async () =>
        {
            await UniTask.Yield();
            float delay = IMgr.Inst._Connect.latency = IMgr.Inst._Connect.CalLatency(sc.elpased_game_tick);
            logger.Info(Define.StrSB("--------------mp_game_batter_input_sc : ", sc.result.ToString(), " : delay = ", delay.ToString()));

            //this.After(delay, () => { IMgr.Inst._InGameMgr.swing.AniSwing(); });
            await UniTask.Delay(System.TimeSpan.FromSeconds(delay));
            IMgr.Inst._InGameMgr.swing.AniSwing();
        });
    }

    public async UniTask RecvBallUpdate(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_hit_ball_update_sc();

        sc.Deserialize(readStream.br);

        Dispatcher.RunOnMainThread(async () =>
        {
            await UniTask.Yield();
            float delay = IMgr.Inst._Connect.latency = IMgr.Inst._Connect.CalLatency(sc.elpased_game_tick);
            logger.Info(Define.StrSB("--------------mp_game_hit_ball_update_sc : ", sc.type.ToString(), " : delay = ", delay.ToString()));

            UnityEngine.Vector3 pos = Define.WVector3(sc.pos_x, sc.pos_y, sc.pos_z);
            UnityEngine.Vector3 vec = Define.WVector3(sc.vec_x, sc.vec_y, sc.vec_z);
            //if (sc.type == (byte)IngameConnect.NetBallUpdateState.Pit) // pit
            //    IMgr.Inst._InGameMgr.pitching.RpcShot(sc.power, pos, vec);
            //else if (sc.type == 2) // hit
            if (sc.type == (byte)IngameConnect.NetBallUpdateState.Hit) // hit
            {
                //UnityEngine.Time.timeScale = 0.1f;
                delay += IMgr.Inst._InGameMgr.swing.swingHitTime;
                //this.After(delay, () => { IMgr.Inst._InGameMgr.swing.RpcHit(sc.power, pos, vec); });
                await UniTask.Delay(System.TimeSpan.FromSeconds(delay));
                IMgr.Inst._InGameMgr.swing.RpcHit(sc.power, pos, vec);
            }
            else if (sc.type == (byte)IngameConnect.NetBallUpdateState.Col && IMgr.Inst._Connect.IsCheckBallResult() == false) // col
            {
                //this.After(delay, () => { IMgr.Inst._BallMgr.ballCollision.RpcBallForce(pos, vec); });
                await UniTask.Delay(System.TimeSpan.FromSeconds(delay));
                IMgr.Inst._BallMgr.ballCollision.RpcBallForce(pos, vec);
            }
            else if (sc.type == (byte)IngameConnect.NetBallUpdateState.GloveIn && IMgr.Inst._Connect.IsCheckBallResult() == false) // col
            {
                //this.After(delay, () => { IMgr.Inst._BallMgr.ballCollision.RpcBallForce(pos, vec); });
                await UniTask.Delay(System.TimeSpan.FromSeconds(delay));
                IMgr.Inst._BallMgr.ballCollision.RpcBallForce(pos, UnityEngine.Vector3.zero);
            }
        });
    }

    public async UniTask RecvLatency(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_latency_sc();

        sc.Deserialize(readStream.br);

        ReqLatency(sc.time_stamp);
    }

    public async UniTask RecvChat(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_chat_sc();

        sc.Deserialize(readStream.br);

        Dispatcher.RunOnMainThread(async () =>
        {
            await UniTask.Yield();
            logger.Info(Define.StrSB("--------------mp_game_chat_sc : ", sc.message.ToString()));

            IMgr.Inst._InGameUIMgr.chatUI.SetChatMessage(sc.message, false);
        });
    }

    public async UniTask RecvDefenceSync(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_defence_sync_sc();

        sc.Deserialize(readStream.br);

        Dispatcher.RunOnMainThread(async () =>
        {
            await UniTask.Yield();
            float delay = IMgr.Inst._Connect.latency = IMgr.Inst._Connect.CalLatency(sc.current_game_tick);
            logger.Info(Define.StrSB("--------------game_defence_sync_sc : delay = ", delay.ToString()));

            await UniTask.Delay(System.TimeSpan.FromSeconds(delay));

            int ballcount = 0;
            TeamFuc fuc = GInfo.myTeam.teamFuc;
            for (int i = 0; i < fuc.defenders.Length; i++)
            {
                fuc.defenders[i].RpcMoveSync(sc.values[i], sc.values[i + 5], sc.values[i + 10]);
                ballcount = i + 10;
            }
            IMgr.Inst._BallMgr.GetTr().position = Define.WVector3(sc.values[++ballcount], sc.values[++ballcount], sc.values[++ballcount]);
            IMgr.Inst._BallMgr.ballMove.rigidbody.velocity = Define.WVector3(sc.values[++ballcount], sc.values[++ballcount], sc.values[++ballcount]);

            //if (IMgr.Inst._Connect.objTest == null)
            //{
            //    IMgr.Inst._Connect.objTest = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube);
            //    Destroy(IMgr.Inst._Connect.objTest.GetComponent<UnityEngine.BoxCollider>());
            //}
            //if (IMgr.Inst._Connect.objTest)
            //{
            //    IMgr.Inst._Connect.objTest.transform.position = IMgr.Inst._BallMgr.GetTr().position;
            //    IMgr.Inst._Connect.objTest.transform.eulerAngles = IMgr.Inst._BallMgr.ballMove.rigidbody.velocity;
            //}
        });
    }

    public async UniTask RecvNetworkFault(ClientToken session, ReadStream readStream)
    {
        var sc = new wcg_game_network_fault_sc();
        sc.Deserialize(readStream.br);

        Dispatcher.RunOnMainThread(async () =>
        {
            await UniTask.Yield();
            logger.Info(Define.StrSB("-------------- game_network_fault_sc : ", sc.uuid.ToString()));

            GMgr.Inst._CommonPopupMgr.CloseAllPopup();
            GMgr.Inst._CommonPopupMgr.OpenPopup(CommonPopup.PopupState.NetWait, "error_message_74");
        });
    }
    #endregion
}
