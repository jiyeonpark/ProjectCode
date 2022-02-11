using System.Text;
using System.Collections.Generic;
using LetsBaseball.Network.Http;
using WCS.Network;
using WCS.Network.Http;

public partial class LobbyServer
{
    public Payloader<wcw_lobby_user_set_sc> UpdateLeader(int leaderID)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_user_set);

        var request = new wcw_lobby_user_set_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.type = 0;
        request.intValue = leaderID;

        return http.Post<wcw_lobby_user_set_sc>(url, request).Callback(
            success: (response) =>
            {
                if (response.result == 0)
                {
                    ServerInfo.user.Info.leaderPlayerID = leaderID;
                }
            });
    }

    public Payloader<wcw_lobby_user_set_sc> UpdateNickName(string nickname, int requiredCost)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_user_set);

        var request = new wcw_lobby_user_set_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.type = 1;
        request.strValue = nickname;

        return http.Post<wcw_lobby_user_set_sc>(url, request).Callback(
            success: (response) =>
            {
                if (response.result == 0)
                {
                    ServerInfo.user.Info.nickName = nickname;
                    ServerInfo.user.ChangeIDCount = response.acc_count;

                    if(ServerInfo.user.ChangeIDCount > 1)
                    {
                        ServerInfo.user.Info.ruby -= requiredCost;
                    }
                }

            });
    }

    public Payloader<wcw_lobby_user_set_sc> UpdateUniform(int color)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_user_set);

        var request = new wcw_lobby_user_set_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.type = 2;
        request.intValue = color;

        return http.Post<wcw_lobby_user_set_sc>(url, request).Callback(
            success: (response) =>
            {
                if (response.result == 0)
                {
                    ServerInfo.user.Info.uniformColor = color;
                }
            });
    }

    public Payloader<wcw_lobby_user_set_sc> UpdateNation(int nation, int requiredCost)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_user_set);

        var request = new wcw_lobby_user_set_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.type = 3;
        request.intValue = nation;

        return http.Post<wcw_lobby_user_set_sc>(url, request).Callback(
            success: (response) =>
            {
                if (response.result == 0)
                {
                    ServerInfo.user.Info.nation = nation;
                    ServerInfo.user.ChangeNationCount = response.acc_count;

                    if (ServerInfo.user.ChangeNationCount > 1)
                    {
                        ServerInfo.user.Info.ruby -= requiredCost;
                    }
                }

            });
    }

    public Payloader<wcw_lobby_user_set_sc> UpdateEmblem(int markIndex, int markColor, int bgIndex, int bgColor)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_user_set);

        var request = new wcw_lobby_user_set_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.type = 4;
        request.intArrValue = new List<int>();
        request.intArrValue.Add(markIndex);
        request.intArrValue.Add(markColor);
        request.intArrValue.Add(bgIndex);
        request.intArrValue.Add(bgColor);

        return http.Post<wcw_lobby_user_set_sc>(url, request).Callback(
            success: (response) =>
            {
                if (response.result == 0)
                {
                    ServerInfo.user.Info.emblemMark = markIndex;
                    ServerInfo.user.Info.emblemMarkColor = markColor;
                    ServerInfo.user.Info.emblemBG = bgIndex;
                    ServerInfo.user.Info.emblemBGColor = bgColor;
                }
            });
    }

    public Payloader<wcw_lobby_user_set_sc> UpdateStadium(int stadiumID)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_user_set);

        var request = new wcw_lobby_user_set_cs(); 
        request.uuid = AuthServer.Inst.Uuid;
        request.type = 5;
        request.intValue = stadiumID;

        return http.Post<wcw_lobby_user_set_sc>(url, request).Callback(
            success: (response) =>
            {
                if (response.result == 0)
                {
                    ServerInfo.user.Info.selectStadium = stadiumID;
                }
            });
    }

    /*
    public Payloader<wcw_lobby_user_set_nickname_sc> UpdateNickName(string nickname)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_team_select);

        var request = new wcw_lobby_user_set_nickname_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.nickname = Encoding.UTF8.GetBytes(nickname);

        return http.Put<wcw_lobby_user_set_nickname_sc>(url, request).Callback(
            success: (response) =>
            { 
            });
    }

    public Payloader<wcw_lobby_user_set_nation_sc> UpdateNation(int nation)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_team_select);

        var request = new wcw_lobby_user_set_nation_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.nation_id = nation;

        return http.Put<wcw_lobby_user_set_nation_sc>(url, request).Callback(
            success: (response) =>
            {
            });
    }

    public Payloader<wcw_lobby_user_set_leader_sc> UpdateLeader(int leaderID)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_team_select);

        var request = new wcw_lobby_user_set_leader_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.leader_id = leaderID;

        return http.Put<wcw_lobby_user_set_leader_sc>(url, request).Callback(
            success: (response) =>
            {
            });
    }

    public Payloader<wcw_lobby_user_set_emblem_sc> UpdateEmblem(List<int> idxlist)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_team_select);

        var request = new wcw_lobby_user_set_emblem_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.ids = idxlist;

        return http.Put<wcw_lobby_user_set_emblem_sc>(url, request).Callback(
            success: (response) =>
            {
            });
    }

    public Payloader<wcw_lobby_user_set_uniform_sc> UpdateUniform(int color)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_team_select);

        var request = new wcw_lobby_user_set_uniform_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.uniform_id = color;

        return http.Put<wcw_lobby_user_set_uniform_sc>(url, request).Callback(
            success: (response) =>
            {
            });
    }
    */
}
