using System;
using System.Collections.Generic;
using UnityEngine;
using LetsBaseball.Network.Http;
using WCS.Network;
using WCS.Network.Http;

public partial class LobbyServer : MonoBehaviour
{
    private static LobbyServer instance = null;
    public static LobbyServer Inst
    {
        get
        {
            if (null == instance)
            {
                GameObject obj = new GameObject("LobbyServer");
                instance = obj.AddComponent<LobbyServer>();
            }

            return instance;
        }
    }
    public static bool IsCreate { get { return instance == null ? false : true; } }

    private BaseLogicHttp http = new BaseLogicHttp();

    private void OnDestroy()
    {
        instance = null;
    }

    public Payloader<wcw_lobby_login_sc> SendLobbyLogin(long uid, string token, string gamebaseid, string version)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_login);

        var request = new wcw_lobby_login_cs();
        request.uuid = uid;
        request.account_name = gamebaseid;
        request.access_token = token;
        request.version = version;

        return http.Post<wcw_lobby_login_sc>(url, request).Callback(
            success: (response) =>
            {
                if (null != response.user)
                {
                    if (0 != response.user.uuid)
                    {
                        AuthServer.Inst.Nickname = response.user.nickname;
                        int level = response.user.level;

                        ServerInfo.user.Info.ID = response.user.uuid;
                        ServerInfo.user.Info.nickName = AuthServer.Inst.Nickname;
                        //if (response.user.introduct != null)
                        //    ServerInfo.user.Info.introducion = Encoding.UTF8.GetString(response.user.introduct).Trim('\0');
                        //ServerInfo.user.Info.userImage = response.user.user_image;
                        ServerInfo.user.Info.nation = response.user.nation;
                        ServerInfo.user.Info.level = response.user.level;
                        ServerInfo.user.Info.leaderPlayerID = response.user.leader;
                        //ServerInfo.user.Info.nowTeamCost = response.user.now_cost;
                        //ServerInfo.user.Info.maxTeamCost = response.user.max_cost;
                        //ServerInfo.user.Info.leagueRanking = response.user.leaque_rank;
                        //ServerInfo.user.Info.globalRanking = response.user.global_rank;
                        //ServerInfo.user.Info.winCount = response.user.point;
                        //ServerInfo.user.Info.tier = response.user.tier;
                        ServerInfo.user.Info.emblemMark = response.user.emblems[0];
                        ServerInfo.user.Info.emblemMarkColor = response.user.emblems[1];
                        ServerInfo.user.Info.emblemBG = response.user.emblems[2];
                        ServerInfo.user.Info.emblemBGColor = response.user.emblems[3];
                        ServerInfo.user.Info.uniformColor = response.user.uniform;
                        ServerInfo.user.Info.maxTrophy = response.user.max_trophy;
                        ServerInfo.user.ChangeIDCount = response.user.change_nickname;
                        ServerInfo.user.ChangeNationCount = response.user.change_nation;
                        if (response.user.stadium > 0)
                            ServerInfo.user.Info.selectStadium = response.user.stadium;
                        else
                            ServerInfo.user.Info.selectStadium = 1;

                        // 캡슐
                        if (response.capsules != null) ServerInfo.capsule.GetServerCapsule(response.capsules);
                        else ServerInfo.capsule.Reset();

                        // 재화
                        if (response.currencies != null) ServerInfo.user.SetCurrencies(response.currencies);

                        // 상점
                        if (response.shop_items != null)
                        {
                            ServerInfo.shop.SetShopInfo(response.shop_items, response.properties);
                        }
                        else
                        {
                            ServerInfo.shop.InitList();
                        }

                        // 메일
                        ServerInfo.SetMail(response.mails);

                        ServerInfo.SetProperties(response.properties);

                        // 시간 설정
                        GMgr.Inst._SystemMgr.SetLoginServerTime(response.server_time);

                        // 라인업 : 선수, 장구류
                        SendLineupAll(() => { SendEquipmentAll(); });
                    }
                }
                else
                {
                    CustomLog.Error(WLogType.all, "SendLobbyLogin : User == null");
                    GMgr.Inst._CommonPopupInfo.RestartPopup("error_message_3");
                }
            },
            fail: (err) =>
            {
                CustomLog.Error(WLogType.all, "SendLobbyLogin : wce_err : ", err);
                switch(err)
                {
                    case wce_err.auth_version_is_not_match:
                        GMgr.Inst._CommonPopupInfo.UpdatePopup("title_update_0005");
                        break;
                }
            });
    }

    public void SendLineupAll(Action ac = null)
    {
        GetLineUp().Callback(
            success: (response) =>
            {
                List<wcs_lineup> lineups = response.lineups;
                GetCardList(wce_item_type.player).Callback(
                success: (response) =>
                {
                    ServerInfo.player.Set(lineups, response.cards);
                    if (ac != null) ac.Invoke();
                });
            });
    }

    public void SendEquipmentAll(Action ac = null)
    {
        GetItem().Callback(
            success: (response) =>
            {
                List<wcs_slot> slots = response.slots;
                GetCardList(wce_item_type.equipment).Callback(
                success: (response) =>
                {
                    ServerInfo.item.Set(slots, response.cards);
                    if (ac != null) ac.Invoke();
                });
            });
    }

    #region cheat
    public Payloader<wcw_lobby_cheat_player_sc> CheatPlayer(int ID, int count)
    {
        CardData data = ServerInfo.player.GetData(ID);
        if (data == null) return null;

        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_cheat_player);

        var request = new wcw_lobby_cheat_player_cs();
        request.uuid = AuthServer.Inst.Uuid;
        if (data.Open) request.cheat_type = 1;
        else request.cheat_type = 0;
        request.player = new wcs_player();
        request.player.tid = ID;
        request.player.type = wce_item_type.player;
        request.player.count = count;
        request.player.level = (short)data.Level;

        return http.Post<wcw_lobby_cheat_player_sc>(url, request).Callback(
            success: (response) =>
            {
                for (int i = 0; i < response.players.Count; i++)
                {
                    data = ServerInfo.player.GetData(response.players[i].tid);
                    if (data == null) continue;
                    data.Count = response.players[i].count;
                    data.Open = true;
                }
                ServerInfo.player.IsReSort = true;
            });
    }

    public Payloader<wcw_lobby_cheat_equipment_sc> CheatEquipment(int ID, int count)
    {
        CardData data = ServerInfo.item.GetData(ID);
        if (data == null) return null;

        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_cheat_equipment);

        var request = new wcw_lobby_cheat_equipment_cs();
        request.uuid = AuthServer.Inst.Uuid;
        if (data.Open) request.cheat_type = 1;
        else request.cheat_type = 0;
        request.equipment = new wcs_equipment();
        request.equipment.tid = ID;
        request.equipment.type = wce_item_type.equipment;
        request.equipment.count = count;
        request.equipment.level = (short)data.Level;

        return http.Post<wcw_lobby_cheat_equipment_sc>(url, request).Callback(
            success: (response) =>
            {
                for (int i = 0; i < response.equipments.Count; i++)
                {
                    data = ServerInfo.item.GetData(response.equipments[i].tid);
                    if (data == null) continue;
                    data.Count = response.equipments[i].count;
                    data.Open = true;
                }
                ServerInfo.item.IsReSort = true;
            });
    }

    public Payloader<wcw_lobby_cheat_player_sc> CheatPlayerAll()
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_cheat_player);

        var request = new wcw_lobby_cheat_player_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.cheat_type = 10;

        return http.Post<wcw_lobby_cheat_player_sc>(url, request).Callback(
            success: (response) =>
            {
                for (int i = 0; i < response.players.Count; i++)
                {
                    CardData data = ServerInfo.player.GetData(response.players[i].tid);
                    if (data == null) continue;
                    data.Count = response.players[i].count;
                    data.Open = true;
                }
                ServerInfo.player.IsReSort = true;
            });
    }

    public Payloader<wcw_lobby_cheat_equipment_sc> CheatEquipmentAll()
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_cheat_equipment);

        var request = new wcw_lobby_cheat_equipment_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.cheat_type = 10;

        return http.Post<wcw_lobby_cheat_equipment_sc>(url, request).Callback(
            success: (response) =>
            {
                for (int i = 0; i < response.equipments.Count; i++)
                {
                    CardData data = ServerInfo.item.GetData(response.equipments[i].tid);
                    if (data == null) continue;
                    data.Count = response.equipments[i].count;
                    data.Open = true;
                }
                ServerInfo.item.IsReSort = true;
            });
    }

    public Payloader<wcw_lobby_cheat_currency_sc> CheatCurrency(wce_currency_type ID, int count)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_cheat_currency);

        var request = new wcw_lobby_cheat_currency_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.cheat_type = 1;
        request.currency = new wcs_currency();
        request.currency.tid = (int)ID;
        request.currency.type = wce_item_type.currency;
        request.currency.count = count;

        return http.Post<wcw_lobby_cheat_currency_sc>(url, request).Callback(
            success: (response) =>
            {
                ServerInfo.user.SetCurrency(response.currency);
                ServerInfo.user.Info.maxTrophy = response.user_max_trophy;
            });
    }

    public Payloader<wcw_lobby_cheat_capsule_sc> CheatCapsule(int ID)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_cheat_capsule);

        var request = new wcw_lobby_cheat_capsule_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.cheat_type = 0;
        request.capsule = new wcs_capsule();
        request.capsule.tid = ID;

        return http.Post<wcw_lobby_cheat_capsule_sc>(url, request).Callback(
            success: (response) =>
            {
                ServerInfo.capsule.AddServerCapsule(response.capsule);
            });
    }

    public Payloader<wcw_lobby_cheat_property_sc> CheatProperty(wcs_property property)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_cheat_capsule);

        var request = new wcw_lobby_cheat_property_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.cheat_type = 1;
        request.property = property;
        return http.Post<wcw_lobby_cheat_property_sc>(url, request).Callback(
            success: (response) =>
            {
                ServerInfo.SetProperty(response.property);
            });
    }

    #endregion
}
