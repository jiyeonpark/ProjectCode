using System;
using System.Collections.Generic;
//using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using WCS;
using WCS.Network;

public partial class IngameServer : MonoBehaviour
{
    #region Singleton
    private static IngameServer _instance = null;

    public static IngameServer Inst
    {
        get
        {
            if (null == _instance)
            {
                GameObject obj = new GameObject("IngameServer");
                _instance = obj.AddComponent<IngameServer>();
            }

            return _instance;
        }
    }
    #endregion



    public delegate UniTask funcGamePacketProcess(ClientToken session, ReadStream stream);
    public delegate bool funcWebPacketProcess(string typename, ReadStream stream);


    Dictionary<wce_cmd, funcGamePacketProcess> _dicGameRecevicePacket = new Dictionary<wce_cmd, funcGamePacketProcess>();
    Dictionary<string, funcWebPacketProcess> _dicWebRecevicePacket = new Dictionary<string, funcWebPacketProcess>();

    //User.Dummy _dummy = null;
    //funcPacketProcess _redirect_func = null;

    private bool IsInit = false;

    private void OnApplicationQuit()
    {
        Disconnected();
        CustomLog.Log(WLogType.all, "OnApplicationQuit()");
    }

    private void OnDisable()
    {
        CustomLog.Log(WLogType.debug, "OnDisable()");
    }

    private void OnDestroy()
    {
        CustomLog.Log(WLogType.debug, "OnDestroy()");
    }

    public bool Disconnected()
    {
        if (type == ClientNetwork.SessionEventType.Connected || (LobbyServer.IsCreate && LobbyServer.Inst.matchState == MatchState.MatchStart))
        {
            if (GameManager.Inst.state == GameState.InGame && type == ClientNetwork.SessionEventType.Connected)
                ReqGameExit();
            DisconnectNetwork();
            LobbyServer.Inst.SendMatchCancel(AuthServer.Inst.Uuid);
            return true;
        }
        return false;
    }

    public void Initialize()
    {
        if (IsInit) return;
        IsInit = true;
        DontDestroyOnLoad(gameObject);

        //게임서버통신

        // server
        _dicGameRecevicePacket.Add(wce_cmd.game_login_sc, RecvLogin);
        _dicGameRecevicePacket.Add(wce_cmd.game_login_complate_sc, RecvLoginComplete);
        _dicGameRecevicePacket.Add(wce_cmd.game_load_complate_sc, RecvLoadComplete);
        _dicGameRecevicePacket.Add(wce_cmd.game_ground_sc, RecvBackground);
        _dicGameRecevicePacket.Add(wce_cmd.game_reconnect_sc, RecvReconnect);
        _dicGameRecevicePacket.Add(wce_cmd.game_matchroom_exit_sc, RecvMatchRoomExit);

        // both
        _dicGameRecevicePacket.Add(wce_cmd.game_start_sc, RecvGameStart);
        _dicGameRecevicePacket.Add(wce_cmd.game_change_trun_sc, RecvChangeTrun);
        _dicGameRecevicePacket.Add(wce_cmd.game_change_view_sc, RecvChangeView);

        // both other
        _dicGameRecevicePacket.Add(wce_cmd.game_skill_sc, RecvGameSkill);
        _dicGameRecevicePacket.Add(wce_cmd.game_batter_result_sc, RecvBallResult);
        _dicGameRecevicePacket.Add(wce_cmd.game_room_exit_sc, RecvGameExit);

        // other
        _dicGameRecevicePacket.Add(wce_cmd.game_pitching_pos_sc, RecvPitPos);
        _dicGameRecevicePacket.Add(wce_cmd.game_batter_pos_sc, RecvHitPos);
        _dicGameRecevicePacket.Add(wce_cmd.game_pitching_input_sc, RecvPitInput);
        _dicGameRecevicePacket.Add(wce_cmd.game_batter_input_sc, RecvSwingInput);
        _dicGameRecevicePacket.Add(wce_cmd.game_hit_ball_update_sc, RecvBallUpdate);
        _dicGameRecevicePacket.Add(wce_cmd.game_latency_sc, RecvLatency);
        _dicGameRecevicePacket.Add(wce_cmd.game_chat_sc, RecvChat);
        _dicGameRecevicePacket.Add(wce_cmd.game_defence_sync_sc, RecvDefenceSync);
        _dicGameRecevicePacket.Add(wce_cmd.game_network_fault_sc, RecvNetworkFault);

        // mine
        _dicGameRecevicePacket.Add(wce_cmd.error_message_sc, RecvError);
        _dicGameRecevicePacket.Add(wce_cmd.game_result_sc, RecvGameResult);
        _dicGameRecevicePacket.Add(wce_cmd.game_ping_sc, RecvGamePing);

    }

    public async UniTask GamePacketProcess(ClientToken session, ReadStream stream)
    {
        try
        {
            funcGamePacketProcess function;

            if (false == _dicGameRecevicePacket.TryGetValue((wce_cmd)stream.command, out function))
            {
                CustomLog.Error(WLogType.all, "GamePacketProcess : message notfound msgtype = ", stream.command);
                return;
            }

            //await function(session, stream).ConfigureAwait(false);
            await function(session, stream);
        }
        catch (Exception e)
        {
            logger.Error(e.Message);
        }
    }


}
