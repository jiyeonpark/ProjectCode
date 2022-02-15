using FrameWork.UI;
using ServicePlatform;

public partial class AuthServer
{
    // 고객센터
    public void ServiceCenter()
    {
        SP.OpenContact((isSuccess, error) =>
        {
            if (isSuccess)
            {
                CustomLog.Log(WLogType.debug, "Contact is closed.");                
            }
            else
            {
                CustomLog.Log(WLogType.debug, "OpenContact is error : ", error);
            }
        });
    }

    // 이용약관
    public void TermsOfService()
    {
        SP.OpenWebView(WebUrlType.TermsOfUse, LSBLocalize.Manager.GetLocalizeString("lobby_menu_0005"), (error) =>
        {
            if (error == 0)
            {
                CustomLog.Log(WLogType.debug, "WebView is closed.");
            }
            else
            {
                CustomLog.Log(WLogType.debug, "OpenWebView error : " + error);
            }
        }, 77, 150, 230);
    }

    // 커뮤니티
    public void Community()
    {
        SP.OpenWebView(WebUrlType.Community, LSBLocalize.Manager.GetLocalizeString("lobby_menu_0005"), (error) =>
        {
            if (error == 0)
            {
                CustomLog.Log(WLogType.debug, "WebView is closed.");
            }
            else
            {
                CustomLog.Log(WLogType.debug, "OpenWebView error : " + error);
            }
        }, 77, 150, 230);
    }
}
