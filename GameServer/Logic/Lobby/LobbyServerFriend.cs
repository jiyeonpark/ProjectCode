using System.Text;
using LetsBaseball.Network.Http;
using WCS.Network;
using WCS.Network.Http;

public partial class LobbyServer
{
    /*
    public Payloader<wcw_lobby_friend_friendlist_sc> GetFriendList()
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_mail_list);

        var request = new wcw_lobby_friend_friendlist_cs();
        request.uuid = AuthServer.Inst.Uuid;

        return http.Put<wcw_lobby_friend_friendlist_sc>(url, request).Callback(
            success: (response) =>
            {
            });
    }

    public Payloader<wcw_lobby_friend_recommended_sc> GetRecommendedList()
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_mail_list);

        var request = new wcw_lobby_friend_recommended_cs();
        request.uuid = AuthServer.Inst.Uuid;

        return http.Put<wcw_lobby_friend_recommended_sc>(url, request).Callback(
            success: (response) =>
            {
            });
    }

    public Payloader<wcw_lobby_friend_received_sc> GetReceivedList()
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_mail_list);

        var request = new wcw_lobby_friend_received_cs();
        request.uuid = AuthServer.Inst.Uuid;

        return http.Put<wcw_lobby_friend_received_sc>(url, request).Callback(
            success: (response) =>
            {
            });
    }

    public Payloader<wcw_lobby_friend_findfriend_sc> FindFriend(string nickname)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_mail_list);

        var request = new wcw_lobby_friend_findfriend_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.findnickname = Encoding.UTF8.GetBytes(nickname);

        return http.Put<wcw_lobby_friend_findfriend_sc>(url, request).Callback(
            success: (response) =>
            {
            });
    }

    public Payloader<wcw_lobby_friend_invite_sc> InviteFriend(long requestUID)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_mail_list);

        var request = new wcw_lobby_friend_invite_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.request_uid = requestUID;

        return http.Put<wcw_lobby_friend_invite_sc>(url, request).Callback(
            success: (response) =>
            {
            });
    }

    public Payloader<wcw_lobby_friend_delete_sc> DeleteFriend(long requestUID)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_mail_list);

        var request = new wcw_lobby_friend_delete_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.request_uid = requestUID;

        return http.Put<wcw_lobby_friend_delete_sc>(url, request).Callback(
            success: (response) =>
            {
            });
    }

    public Payloader<wcw_lobby_friend_reject_sc> RejectReceivedRequest(long requestUID)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_mail_list);

        var request = new wcw_lobby_friend_reject_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.request_uid = requestUID;

        return http.Put<wcw_lobby_friend_reject_sc>(url, request).Callback(
            success: (response) =>
            {
            });
    }

    public Payloader<wcw_lobby_friend_friendlymatch_sc> RequestFriendlyMatch(long requestUID)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_mail_list);

        var request = new wcw_lobby_friend_friendlymatch_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.request_uid = requestUID;

        return http.Put<wcw_lobby_friend_friendlymatch_sc>(url, request).Callback(
            success: (response) =>
            {
            });
    }
    */
}
