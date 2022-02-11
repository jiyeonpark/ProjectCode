using LetsBaseball.Network.Http;
using WCS.Network;
using WCS.Network.Http;

public partial class LobbyServer
{
    public Payloader<wcw_lobby_mail_get_sc> GetMail()
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_mail_get);

        var request = new wcw_lobby_mail_get_cs();
        request.uuid = AuthServer.Inst.Uuid;

        return http.Post<wcw_lobby_mail_get_sc>(url, request).Callback(
            success: (response) =>
            {
                ServerInfo.SetMail(response.mails);
            });
    }

    public Payloader<wcw_lobby_mail_receive_sc> GetMailReward(long mail_id)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_mail_receive);

        var request = new wcw_lobby_mail_receive_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.mail_id = mail_id;

        return http.Post<wcw_lobby_mail_receive_sc>(url, request).Callback(
            success: (response) =>
            {
                // response에 아이템이랑 대체보상(아이템이 꽉 찼을경우) 존재
                // 현재 재화가 중복으로 올라가고 있음. GoodsProduction에서 올려주는 부분을 어떻게 처리할지 상의 한 후 수정
                //ServerInfo.user.SetCurrencies(response.currencies);
            });
    }
}
