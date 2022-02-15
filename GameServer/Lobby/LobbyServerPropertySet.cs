using System.Text;
using System.Collections.Generic;
using LetsBaseball.Network.Http;
using WCS.Network;
using WCS.Network.Http;

public partial class LobbyServer
{
    public Payloader<wcw_lobby_property_set_sc> LobbyPropertySet(wce_property_type type, int tid = -1)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_property_set);

        var request = new wcw_lobby_property_set_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.type = type;
        request.tid = tid;

        return http.Post<wcw_lobby_property_set_sc>(url, request).Callback(
            success: (response) =>
            {
                ServerInfo.SetProperty(response.property);
            });
    }
}
