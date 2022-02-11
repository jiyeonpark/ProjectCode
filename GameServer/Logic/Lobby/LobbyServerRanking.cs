using LetsBaseball.Network.Http;
using WCS.Network;
using WCS.Network.Http;

public partial class LobbyServer
{
    public Payloader<wcw_lobby_rank_get_sc> GetRanking(wce_rank_type type)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_rank_get);

        var request = new wcw_lobby_rank_get_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.type = type;

        return http.Post<wcw_lobby_rank_get_sc>(url, request).Callback(
            success: (response) =>
            {

            });
    }

    public Payloader<wcw_lobby_rank_get_scroll_sc> GetRankingScroll(wce_rank_type type, byte indexType, int index)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_rank_getscroll);

        var request = new wcw_lobby_rank_get_scroll_cs();
        request.index_type = indexType;
        request.type = type;
        request.index = index;

        return http.Post<wcw_lobby_rank_get_scroll_sc>(url, request).Callback(
            success: (response) =>
            {

            });
    }

    public Payloader<wcw_lobby_rank_get_detail_sc> GetUserDetail(long targetUUID)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_rank_getdetail);

        var request = new wcw_lobby_rank_get_detail_cs();
        request.uuid = targetUUID;

        return http.Post<wcw_lobby_rank_get_detail_sc>(url, request).Callback(
            success: (response) =>
            {

            });
    }
}
