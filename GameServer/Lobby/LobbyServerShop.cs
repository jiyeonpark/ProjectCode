using LetsBaseball.Network.Http;
using UnityEngine;
using WCS.Network;
using WCS.Network.Http;
using System.Collections.Generic;
using System.Linq;
#region _GameBase Pay
using ServicePlatform;
using UnityEngine.Networking;
using System;
using LitJson;
#endregion _GameBase Pay

public partial class LobbyServer
{
    public Payloader<wcw_lobby_shop_item_buy_sc> ShopBuyItem(wce_shop_type itemtype, long itemid)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLobby, PacketUrl.URL_lobby_shop_item_buy);

        var request = new wcw_lobby_shop_item_buy_cs();
        request.uuid = AuthServer.Inst.Uuid;
        request.type = itemtype;
        request.shop_id = itemid;
        request.access_token = string.Empty;
        request.payment_seq = string.Empty;

//#if !UNITY_EDITOR
        //pay 결제 내용 추가.
        if (false == string.IsNullOrEmpty( ServerInfo.shop.ProductIdx ))
        {
            ConsumeItem consumeitem = SP.consumeItems.Find(x => 0 == string.Compare(x.gamebaseProductId, ServerInfo.shop.ProductIdx));
            //if ( null!= consumeitem)
            {
                request.access_token = consumeitem.purchaseToken;
                request.payment_seq = consumeitem.paymentSeq;
            }
            CustomLog.Log(WLogType.donghee, $"========== LobbyServer wcw_lobby_shop_item_buy_sc access_token[{request.access_token}] payment_seq[{request.payment_seq}] ");
        }
//#endif

        return http.Post<wcw_lobby_shop_item_buy_sc>(url, request).Callback(
            success:(response)=>
            {
                if (0 == response.result)
                {
                    //response.property
                    //response.purchase_items
                    //response.currencies
                    ServerInfo.user.SetCurrencies(response.currencies);
                }
                else
                {
                    //error
                    CustomLog.Error(WLogType.all, "ShopBuyItem : wce_err : ", response.result);
                }
            },
            fail: (err) =>
            {
                CustomLog.Error(WLogType.all, "ShopBuyItem : wce_err : ", err);
            });
    }

}
