using LetsBaseball.Network.Http;
using UnityEngine;
using WCS.Network;
using WCS.Network.Http;
using System.Collections.Generic;
using System.Linq;

public partial class LobbyServer
{
    //public void ChangeLineUp 
    public Payloader<wcw_lobby_lineup_set_sc> ChangeLineUp( wce_lineup_type linetype, int []batorder, int []lineup )
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_lineup_set);

        var request = new wcw_lobby_lineup_set_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.index = 0;
        request.type = linetype;
        request.lineups = lineup.ToList<int>();
        request.batters = batorder.ToList<int>();

        return http.Post<wcw_lobby_lineup_set_sc>(url, request);
        //return http.Put<wcw_lobby_lineup_set_sc>(url, request).Callback(
        //    complete: (response) =>
        //    {
        //        // 완료 : 실패, 성공 여부와 별개
        //        //if ((int)wce_err.none == response.result)
        //        //{
        //        //    CustomLog.Log(WLogType.debug, "========== Server ChangeLineUp Success!!");
        //        //}
        //        //else
        //        //{
        //        //    CustomLog.Error(WLogType.all, "========== Server ChangeLineUp :", response.result.ToString());
        //        //}
        //    },
        //    success: (response) =>
        //    {
        //        // 성공
        //    },
        //    fail: (err) =>
        //    {
        //        // 실패 : wce_err
        //    },
        //    error: (err) =>
        //    {
        //        // http 전송자체가 실패
        //    });
    }

    public Payloader<wcw_lobby_slot_set_sc> ChangeItem(int itempos, int batindex, int gloveindex)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_slot_set);

        var request = new wcw_lobby_slot_set_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.index = 0;
        request.position = (wce_lineup_pos)itempos;
        request.bat = batindex;
        request.glove = gloveindex;

        return http.Post<wcw_lobby_slot_set_sc>(url, request);
    }

    public Payloader<wcw_lobby_lineup_get_sc> GetLineUp()
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_lineup_get);

        var request = new wcw_lobby_lineup_get_cs();
        request.uuid = AuthServer.Inst.Uuid;

        return http.Post<wcw_lobby_lineup_get_sc>(url, request);
    }

    public Payloader<wcw_lobby_slot_get_sc> GetItem()
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_slot_get);

        var request = new wcw_lobby_slot_get_cs();
        request.uuid = AuthServer.Inst.Uuid;

        return http.Post<wcw_lobby_slot_get_sc>(url, request);
    }

    public Payloader<wcw_lobby_card_get_sc> GetCardList(wce_item_type type)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_card_get);

        var request = new wcw_lobby_card_get_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.type = type;

        return http.Post<wcw_lobby_card_get_sc>(url, request);
    }

    public Payloader<wcw_lobby_card_levelup_sc> LevelUpCard(int card_id, int card_level, wce_item_type type)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_card_levelup);

        var request = new wcw_lobby_card_levelup_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.card_type = type;
        request.tid = card_id;

        return http.Post<wcw_lobby_card_levelup_sc>(url, request);
    }
    /*
    public Payloader<wcw_lobby_item_change_sc> ChangeItem(int item_id, wce_equip_pos equip_pos)
    { 
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_team_select);

        var request = new wcw_lobby_item_change_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.item_id = item_id;
        request.equip_slot = equip_pos;

        return http.Put<wcw_lobby_item_change_sc>(url, request).Callback(
            success: (response) =>
            {
            });
    }



    public Payloader<wcw_lobby_player_change_sc> ChangePlayer(int card_id)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_team_select);

        var request = new wcw_lobby_player_change_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.card_id = card_id;

        return http.Put<wcw_lobby_player_change_sc>(url, request).Callback(
            success: (response) =>
            {
            });
    }


    */
}
