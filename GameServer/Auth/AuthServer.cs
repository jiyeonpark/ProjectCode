using System;
using System.Text;
using UnityEngine;
using LetsBaseball.Network.Http;
using WCS.Network;
using WCS.Network.Http;
using ServicePlatform;

public partial class AuthServer : MonoBehaviour
{
    private static AuthServer instance = null;
    public static AuthServer Inst
    {
        get
        {
            if (null == instance)
            {
                GameObject obj = new GameObject("AuthServer");
                instance = obj.AddComponent<AuthServer>();
            }

            return instance;
        }
    }

    private BaseLogicHttp http = new BaseLogicHttp();

    [Space]
    public string ID;
    public long Uuid;
    public int Gid;
    public string AccessToken;
    public string Nickname = "";
    public string gamebaseAccessToken;

    private void OnDestroy()
    {
        instance = null;
    }

    public void Initialize()
    {
        DontDestroyOnLoad(gameObject);

        SendStreamPool.instance.Initialize(32, 2);
        ReadStreamPool.instance.Initialize(32, 2);

        IngameServer.Inst.Initialize();

#if UNITY_EDITOR
        if (GameManager.Inst.testID != "")
        {
            gamebaseAccessToken = GameManager.Inst.testID;
            CustomLog.Log(WLogType.debug, "setting GameManager.Inst.testID !!!");
            return;
        }
#endif

        // 게임베이스 시작점(초기화)            
        // isSuccess : 성공여부
        // error : 에러코드            
        SP.Initialize((isSuccess, error) =>
        {
            if (isSuccess)
            {
                // 옵션 정보 로드
                XmlManager.sInstance.LoadOptionInfoXML();

                // 게임베이스 초기화 완료.
                var status = SP.LaunchingInfo.launching.status;

                if (SP.IsPlayable(status.code))
                {
                    CheckLaunchingStatus(status.code);
                }
                else
                {
                    CheckLaunchingStatus(status.code);
                }
            }
            else
            {
                CheckLaunchingError(error);
            }
        });
    }

    #region SP
    // 구글 인증 : 소셜 기타 등등 체크..
    void CheckLaunchingStatus(int statusCode)
    {
        SP.AddLog(Define.StrSB("Initialization succeed. status is ", statusCode));

        switch (statusCode)
        {
            case Toast.Gamebase.GamebaseLaunchingStatus.IN_SERVICE:
            case Toast.Gamebase.GamebaseLaunchingStatus.IN_SERVICE_BY_QA_WHITE_LIST:
            case Toast.Gamebase.GamebaseLaunchingStatus.RECOMMEND_UPDATE:
            case Toast.Gamebase.GamebaseLaunchingStatus.IN_TEST:
            case Toast.Gamebase.GamebaseLaunchingStatus.IN_REVIEW:
            case Toast.Gamebase.GamebaseLaunchingStatus.IN_BETA:
                {
                    // 플레이 가능상태
                    // 로그인 프로세스 진행.
                    StartLoginGamebase();
                }
                break;
            case Toast.Gamebase.GamebaseLaunchingStatus.REQUIRE_UPDATE:
            case Toast.Gamebase.GamebaseLaunchingStatus.BLOCKED_USER:
            case Toast.Gamebase.GamebaseLaunchingStatus.TERMINATED_SERVICE:
            case Toast.Gamebase.GamebaseLaunchingStatus.INSPECTING_SERVICE:
            case Toast.Gamebase.GamebaseLaunchingStatus.INSPECTING_ALL_SERVICES:
            case Toast.Gamebase.GamebaseLaunchingStatus.INTERNAL_SERVER_ERROR:
                {
                    // 플레이 불가                       
                }
                break;
        }
    }

    void CheckLaunchingError(int errorCode)
    {
        SP.AddLog(Define.StrSB("Initialization failed. error is ", errorCode));

        switch (errorCode)
        {
            case -100:
                {
                    // DataManager의 초기화 실패.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.NOT_INITIALIZED:
                {
                    // Gamebase 초기화 실패.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.NOT_LOGGED_IN:
                {
                    // 로그인이 필요.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.INVALID_PARAMETER:
                {
                    // 잘못된 파라미터.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.INVALID_JSON_FORMAT:
                {
                    // JSON 포맷 오류.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.USER_PERMISSION:
                {
                    // 권한없음.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.NOT_SUPPORTED:
                {
                    // 지원하지 않는 기능.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.LAUNCHING_SERVER_ERROR:
                {
                    // 런칭서버가 내려준 항목에 약관 관련 내용이 없는 경우에 발생하는 에러.
                    // 정상적인 상황이 아니므로 Gamebase 담당자에게 문의.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.UI_TERMS_ALREADY_IN_PROGRESS_ERROR:
                {
                    // 이전에 호출된 Terms API 가 아직 완료되지 않았음.
                    // 잠시 후 다시 시도.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.UI_TERMS_ANDROID_DUPLICATED_VIEW:
                {
                    // 약관 웹뷰가 아직 종료되지 않았는데 다시 호출.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.WEBVIEW_TIMEOUT:
                {
                    // 약관 웹뷰 표시 중 타임아웃이 발생.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.WEBVIEW_HTTP_ERROR:
                {
                    // 약관 웹뷰 오픈 중 HTTP 에러가 발생.
                }
                break;
            default: break;
        }
    }

    void StartLoginGamebase()
    {
        // 최근에 접속정보가 있다면 자동접속 처리하는 루틴
        // isSuccess : 성공여부
        // userId : 접속한 유저의 게임베이스 id
        // withDrawalDate : 탈퇴유예 상태인 유저의 date값
        // error : 에러코드
        SP.LoginForLastLoggedInProvider((isSuccess, userId, withDrawalDate, error) =>
        {
            if (isSuccess)
            {
                SP.AddLog(Define.StrSB("Login succeed : ", userId.ToString()));

                CheckWithdrawal(userId, withDrawalDate, false);

                //if (withDrawalDate != 0)
                //{
                //    // 탈퇴 유예기간 유저임을 알려줘야 한다.
                //    DateTime resultTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((double)withDrawalDate);
                //    SP.AddLog(Define.StrSB("withDrawalDate : ", resultTime.Year, " / ", resultTime.Month, " / ", resultTime.Day));
                //}

                //if(SP.termsPushInfo.enabled)
                //{
                //    GameManager.Inst.optionInfo.RegisterPushInfo(SP.termsPushInfo);
                //}              

                //// 다음 게임진행 스텝으로 넘어가자.
                //// userId : 게임베이스에 생성된 계정의 uid.
                //// 서버에 접속을 요청한다.
                //// ...
                //ID = Nickname = GameManager.Inst.testID = userId;
                //gamebaseAccessToken = SP.GetAccessToken();
                //SP.AddLog(Define.StrSB("GetAccessToken() : ", gamebaseAccessToken));
                //GameManager.Inst.startProcess.ChangeProcess(StartProcess.Process.Login_Server);
            }
            else
            {
                CheckLoginError(error);

                if (error == -101)
                {
                    // LSB 8월 세일즈버전
                    // 최초 실행시에는 여길 탈것이다.
                    // 게스트 로그인 요청을 바로 호출하자
                    RequestLoginGamebase(Toast.Gamebase.GamebaseAuthProvider.GUEST);
                }
            }
        });
    }

    void RequestLoginGamebase(string requestProvider)
    {
        // 원하는 IdP계정 로그인을 클릭했을때 처리하는 루틴
        // isSuccess : 성공여부
        // userId : 접속한 유저의 게임베이스 id
        // withDrawalDate : 탈퇴유예 상태인 유저의 date값
        // error : 에러코드
        SP.LoginWithProviderName(requestProvider, (isSuccess, userId, withDrawalDate, error) =>
        {
            if (isSuccess)
            {
                SP.AddLog(Define.StrSB("Login succeed : ", userId.ToString()));

                CheckWithdrawal(userId, withDrawalDate, false);

                //if (withDrawalDate != 0)
                //{
                //    // 탈퇴 유예기간 유저임을 알려줘야 한다.
                //    DateTime resultTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((double)withDrawalDate);
                //    SP.AddLog(Define.StrSB("withDrawalDate : ", resultTime.Year, " / ", resultTime.Month, " / ", resultTime.Day));
                //}

                //if (SP.termsPushInfo.enabled)
                //{
                //    GameManager.Inst.optionInfo.RegisterPushInfo(SP.termsPushInfo);
                //}

                //// 다음 게임진행 스텝으로 넘어가자.
                //// userId : 게임베이스에 생성된 계정의 uid.
                //// 서버에 접속을 요청한다.
                //// ...
                //ID = Nickname = GameManager.Inst.testID = userId;
                //gamebaseAccessToken = SP.GetAccessToken();
                //SP.AddLog(Define.StrSB("GetAccessToken() : ", gamebaseAccessToken));
                //GameManager.Inst.startProcess.ChangeProcess(StartProcess.Process.Login_Server);
            }
            else
            {
                CheckLoginError(error);
                GMgr.Inst._CommonPopupMgr.OpenPopupTxt(CommonPopup.PopupState.BoxNone, Define.StrSB("[ServicePlatform] error code : ", error.ToString()));
            }
        });
    }

    void CheckLoginError(int errorCode)
    {
        SP.AddLog(Define.StrSB("Login failed. error is ", errorCode));

        switch (errorCode)
        {
            case -101:
                {
                    // 최근 접속한 정보가 없다.                                      
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.SOCKET_ERROR:
            case Toast.Gamebase.GamebaseErrorCode.SOCKET_RESPONSE_TIMEOUT:
                {
                    // 일시적인 네트워크 문제, 재시도 요청.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.BANNED_MEMBER:
                {
                    GameManager.Inst.Exit();

                    //// 밴당한 유저다.
                    //// * 게임플레이를 못하게 해야함.
                    //Toast.Gamebase.GamebaseResponse.Auth.BanInfo bannedUser = SP.GetBannedUserInfo();

                    //if (bannedUser != null)
                    //{
                    //    DateTime beginDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((double)bannedUser.beginDate);
                    //    DateTime endDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((double)bannedUser.endDate);

                    //    SP.AddLog(Define.StrSB("[BannedUser]"));
                    //    SP.AddLog(Define.StrSB("userId : ", bannedUser.userId));
                    //    SP.AddLog(Define.StrSB("beginDate : ", beginDate.Year, " / ", beginDate.Month, " / ", beginDate.Day));
                    //    SP.AddLog(Define.StrSB("endDate : ", endDate.Year, " / ", endDate.Month, " / ", endDate.Day));
                    //    SP.AddLog(Define.StrSB("reason : ", bannedUser.message));
                    //}
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.AUTH_TOKEN_LOGIN_FAILED:
                {
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.AUTH_TOKEN_LOGIN_INVALID_TOKEN_INFO:
                {
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.AUTH_TOKEN_LOGIN_INVALID_LAST_LOGGED_IN_IDP:
                {
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.AUTH_IDP_LOGIN_FAILED:
                {
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.AUTH_IDP_LOGIN_INVALID_IDP_INFO:
                {
                }
                break;
            default: break;
        }
    }

    public void LogOutGamebase(Action ac)
    {
        // 로그아웃
        SP.LogOut((isSuccess, error) =>
        {
            if (isSuccess)
            {
                SP.AddLog("LogOut succeed.");
            }
            else
            {
                CheckLogOutError(error);
            }
            ac.Invoke();
        });
    }

    void CheckLogOutError(int errorCode)
    {
        SP.AddLog(Define.StrSB("LogOut failed. error is ", errorCode));

        switch (errorCode)
        {
            case Toast.Gamebase.GamebaseErrorCode.SOCKET_ERROR:
            case Toast.Gamebase.GamebaseErrorCode.SOCKET_RESPONSE_TIMEOUT:
                {
                    // 일시적인 네트워크 문제, 재시도 요청.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.AUTH_LOGOUT_FAILED:
                {
                }
                break;
            default: break;
        }
    }
    #endregion

    // 서버 데이터 체크 ( version & path )
    public void CheckServerInfo(Action ac = null)
    {
        if (GameManager.Inst.FixServerTestMode != ServerVersion.service)
        {
            BaseLogicHttp.urlLogin = BaseLogicHttp.UrlLogin;
            BaseLogicHttp.serverGid = BaseLogicHttp.ServerGid;
            StartAuth(ac);
        }
        else
        {
            string url = "http://front.wisecat.games/serverinfo.json";
            http.Get(new Uri(url), (req, res) =>
            {
                CustomLog.Log(WLogType.all, "res.Message = ", res.Message, "\n", res.DataAsText);
                WepServerInfo info = Newtonsoft.Json.JsonConvert.DeserializeObject<WepServerInfo>(res.DataAsText);

                int groupid = 0;
                string loginserver = "";
                for (int i = 0; i < info.region.Length; i++)
                {
                    if (info.region[i].version == GameManager.Inst.startProcess.GetVersion())
                    {
                        groupid = info.region[i].group_id;
                        loginserver = info.region[i].login_server;
                        break;
                    }
                }
                BaseLogicHttp.urlLogin = loginserver;
                BaseLogicHttp.serverGid = groupid;
                CustomLog.Log(WLogType.all, "ok? => ", loginserver);
                StartAuth(ac);
            });
        }
    }

    // 서버 인증 & 로그인 시작
    public void StartAuth(Action ac = null)
    {
        CustomLog.Log(WLogType.debug, "Server Login Start gamebase ID : ", ID.ToString());
        CustomLog.Log(WLogType.debug, "Server Login Start gamebase token : ", gamebaseAccessToken.ToString());
        GetAuth(ID, gamebaseAccessToken, GameManager.Inst.startProcess.GetVersion()).Callback( success: (response) => { StartLobbyPath(ac); });
    }
    void StartLobbyPath(Action ac = null)
    {
        GetLobbyPath(ID, Gid).Callback( success: (response) => { StartLobbyLogin(ac); GetPayInfo(); });
    }
    void StartLobbyLogin(Action ac = null)
    {
        LobbyServer.Inst.SendLobbyLogin(Uuid, AccessToken, ID, GameManager.Inst.startProcess.GetVersion()).Callback(success: (response) => { ac.Invoke(); });
    }

    void GetPayInfo()
    {
        SP.RequestItemListPurchasable((isSuccess, purchasableItems, error) =>
        {
            if (isSuccess)
            {
                if (SP.purchasableItems.Count == 0)
                {
                    CustomLog.Log(WLogType.donghee, "There are no items available for purchase. Register your product in the TOAST Console");
                }
                else
                {
                    for (int i = 0; i < SP.purchasableItems.Count; i++)
                    {
                        CustomLog.Log(WLogType.donghee, "Id:[", SP.purchasableItems[i].gamebaseProductId, "] Price:[", SP.purchasableItems[i].price, "] Name:[", SP.purchasableItems[i].name, "]");
                        CustomLog.Log(WLogType.donghee, "LocalPrice:[", SP.purchasableItems[i].localizedPrice, "] LocalTitle:[", SP.purchasableItems[i].localizedTitle, "] LocalDesc:[", SP.purchasableItems[i].localizedDescription, "]");
                    }
                }
            }
            else
            {
                CustomLog.Log(WLogType.donghee, "SetPayInfo RequestItemListPurchasable Fail!!!!!");
            }
        });

    }



    public Payloader<wcw_login_loginauth_sc> GetAuth(string id, string gamebasetoken, string version)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLogin, PacketUrl.URL_login_loginauth);

        var request = new wcw_login_loginauth_cs();
        request.account_name = id;
        request.access_token = gamebasetoken;
        request.version = version;

        return http.Post<wcw_login_loginauth_sc>(url, request).Callback(
            success: (response) =>
            {
                ID = response.user.account_name;
                Uuid = response.user.uuid;

                CustomLog.Log(WLogType.debug, "uuid : ", response.user?.uuid, ", server : ", response.servers?.Count);

                if (null != response.servers)
                {
                    for (int i = 0; i < response.servers.Count; i++)
                    {
                        if (response.servers[i].gid == BaseLogicHttp.serverGid)   // 맞는 gid로 접속..
                        {
                            Gid = response.servers[i].gid;
                            break;
                        }
                    }
                }
            },
            fail: (err) =>
            {
                CustomLog.Error(WLogType.all, "GetAuth : wce_err : ", err.ToString());
                switch (err)
                {
                    case wce_err.auth_version_is_not_match:
                        GMgr.Inst._CommonPopupInfo.UpdatePopup("title_update_0005");
                        break;
                }
            });
    }

    public Payloader<wcw_login_lobbyaddr_sc> GetLobbyPath(string id, int gid)
    {
        string url = Define.StrSB(BaseLogicHttp.urlLogin, PacketUrl.URL_login_lobbyaddr);

        var request = new wcw_login_lobbyaddr_cs();
        request.account_name = id;
        request.gid = gid;

        return http.Post<wcw_login_lobbyaddr_sc>(url, request).Callback(
            success: (response) =>
            {
                AccessToken = response.access_token;
                BaseLogicHttp.urlLobby = response.server_info.ip;
                BaseLogicHttp.lobbyPort = response.server_info.port;

                CustomLog.Log(WLogType.debug, "access token : ", response.access_token);
            },
            fail: (err) =>
            {
                CustomLog.Error(WLogType.all, "GetLobbyPath : wce_err : ", err);
            });
    }
}

[System.Serializable]
public class WepServerPath
{
    public string version;
    public int group_id;
    public string login_server;
}

[System.Serializable]
public class WepServerInfo
{
    public WepServerPath[] region;
}
