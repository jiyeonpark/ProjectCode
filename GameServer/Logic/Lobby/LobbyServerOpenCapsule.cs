using System.Text;
using System.Collections.Generic;
using LetsBaseball.Network.Http;
using WCS.Network;
using WCS.Network.Http;

public partial class LobbyServer
{
    public Payloader<wcw_lobby_capsule_open_sc> CapsuleOpen(long capsuleID, bool paidRuby)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_capsule_open);

        var request = new wcw_lobby_capsule_open_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.capsule_id = capsuleID;
        request.use_ruby = paidRuby;

        return http.Post<wcw_lobby_capsule_open_sc>(url, request).Callback(
            success: (response) =>
            {
                ServerInfo.user.SetCurrencies(response.currencies);
                ServerInfo.SetProperties(response.properties);
            });
    }

    public Payloader<wcw_lobby_capsule_prep_sc> CapsuleOpenStart(long capsuleID)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_capsule_prep);

        var request = new wcw_lobby_capsule_prep_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.capsule_id = capsuleID;

        return http.Post<wcw_lobby_capsule_prep_sc>(url, request).Callback(
            success: (response) =>
            {

            });
    }

    public Payloader<wcw_lobby_capsule_standby_sc> CapsuleStandby(long capsuleID)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_capsule_standby);

        var request = new wcw_lobby_capsule_standby_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.capsule_id = capsuleID;

        return http.Post<wcw_lobby_capsule_standby_sc>(url, request).Callback(
            success: (response) =>
            {

            });
    }
}
