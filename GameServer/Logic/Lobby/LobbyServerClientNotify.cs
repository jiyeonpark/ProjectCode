using LetsBaseball.Network.Http;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WCS.Network;
using WCS.Network.Http;

public partial class LobbyServer
{
    public Payloader<wcw_lobby_notify_client_sc> RequestClientNotify(wce_client_notify_type type, long value = -1)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_notify);

        var request = new wcw_lobby_notify_client_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.type = type;
        if(value > 0) request.value = value;

        return http.Post<wcw_lobby_notify_client_sc>(url, request).Callback(
            success: (response) =>
            {
                ServerInfo.SetProperties(response.properties);

                switch(type)
                {
                    case wce_client_notify_type.time_update: 
                        GMgr.Inst._SystemMgr.SetServerTime(response.value);
                        break;
                }
            });
    }
}
