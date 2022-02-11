using System;
using ServicePlatform;

public partial class AuthServer
{
    public enum Login
    {
        Facebook,
        Google,
        Apple,
        Customer,
    }
    public string loggedInProvider = "";
    public string requestProviderTemp = "";

    public void AccountLogin(Login loginType, Action ac)
    {
        if (IsAccountLogin()) return;
        switch(loginType)
        {
            case Login.Facebook: AddMappingGamebase(Toast.Gamebase.GamebaseAuthProvider.FACEBOOK, ac); break;
            case Login.Google: AddMappingGamebase(Toast.Gamebase.GamebaseAuthProvider.GOOGLE, ac); break;
            case Login.Apple: AddMappingGamebase(Toast.Gamebase.GamebaseAuthProvider.APPLEID, ac); break;
        }
    }

    #region SP
    public bool IsAccountLogin()
    {
        loggedInProvider = SP.GetLastLoggedInProvider();
        if (loggedInProvider == "guest") return false;
        else return true;
    }

    // 게스트에서 소셜계정 연동시 호출하는 함수
    void AddMappingGamebase(string requestProvider, Action ac)
    {
        requestProviderTemp = requestProvider;
        SP.AddMapping(requestProvider, (isSuccess, error) =>
        {
            if (isSuccess)
            {
                // 맵핑성공
                //SP.AddLog(Define.StrSB("AddMapping succeed. ", requestProvider));

                // 게임 베이스 소셜 계정 연동이 성공 되었다.
                //GameManager.Inst.startProcess.ChangeProcess(StartProcess.Process.CI);

                //ac.Invoke();

                UpdateDisplayName(ac);
            }
            else
            {
                CheckMappingError(error, ac);
            }

            //ActiveOption(true);
        });
    }

    void UpdateDisplayName(Action ac = null)
    {
        SP.Behaviour.RequestIdPProfile((isSuccess, outputName) =>
        {
            if (isSuccess == false)
            {
                SP.UpdateDisplayNameByAuthProviderProfile();
            }

            ac?.Invoke();
        });
    }

    // 이미 연동된 소셜계정으로 다시 로그인을 요청하는 함수
    void AddMappingGamebaseReConnect(string requestProvider, Action ac)
    {
        SP.AddMappingReConnect(requestProvider, (isSuccess, userId, withDrawalDate, error) =>
        {
            if (isSuccess)
            {
                // 기존 계정으로 다시 로그인 성공 되었다.

                CheckWithdrawal(userId, withDrawalDate, true, ac);

                //if (withDrawalDate != 0)
                //{
                //    // 탈퇴 유예기간 유저임을 알려줘야 한다.
                //    DateTime resultTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((double)withDrawalDate);
                //    SP.AddLog(Define.StrSB("withDrawalDate : ", resultTime.Year, " / ", resultTime.Month, " / ", resultTime.Day));
                //}

                ////SP.CheckTermsPushInfo();  // 여기에서는 처리하지 말자

                //// 서버에 재접속을 처리하자
                //GameManager.Inst.startProcess.ChangeProcess(StartProcess.Process.CI);

                ////ActiveOption(true);

                //ac.Invoke();
            }
            else
            {
                CheckMappingError(error, ac);
            }
        });
    }

    void CheckWithdrawal(string userId, long withDrawalDate, bool reConnect = false, Action reConnectCallback = null)
    {
        if (withDrawalDate != 0)
        {
            // 탈퇴 유예기간 유저임을 알려줘야 한다.
            DateTime resultTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((double)withDrawalDate);            
            string message = Define.StrSB("You are applying for withdrawal.\nWithdraw date : ", resultTime.Year, " / ", resultTime.Month, " / ", resultTime.Day, "\nWould you like to request a release?");

            GMgr.Inst._CommonPopupMgr.OpenPopupTxt(CommonPopup.PopupState.BoxChoice, message, (result) =>
            {
                switch (result)
                {
                    case CommonPopup.PopupCallback.OK:
                        {
                            SP.CancelWithdrawal((isSuccess, error) =>
                            {
                                if (isSuccess)
                                {                                    
                                    GMgr.Inst._CommonPopupMgr.OpenPopupTxt(CommonPopup.PopupState.Line, "It has been released normally.", 2.0f);

                                    LoginNextProcess(userId, reConnect, reConnectCallback);
                                }
                                else
                                {
                                    GameManager.Inst.Exit();
                                }
                            });
                        }
                        break;
                    case CommonPopup.PopupCallback.Cancel:
                        {
                            GameManager.Inst.Exit();
                        }
                        break;
                }
            });
        }
        else
        {
            LoginNextProcess(userId, reConnect, reConnectCallback);
        }
    }

    void LoginNextProcess(string userId, bool reConnect = false, Action reConnectCallback = null)
    {
        if(reConnect)
        {
            GameManager.Inst.startProcess.ChangeProcess(StartProcess.Process.CI);

            //reConnectCallback?.Invoke();
            UpdateDisplayName(reConnectCallback);
        }
        else
        {
            if (SP.termsPushInfo.enabled)
            {
                GameManager.Inst.optionInfo.RegisterPushInfo(SP.termsPushInfo);
            }

            UpdateDisplayName();

            // 다음 게임진행 스텝으로 넘어가자.
            // userId : 게임베이스에 생성된 계정의 uid.
            // 서버에 접속을 요청한다.
            // ...
            ID = Nickname = GameManager.Inst.testID = userId;
            gamebaseAccessToken = SP.GetAccessToken();
            SP.AddLog(Define.StrSB("GetAccessToken() : ", gamebaseAccessToken));
            GameManager.Inst.startProcess.ChangeProcess(StartProcess.Process.Login_Server);
        }

        //PerftestManager.SetId(userId);  // 앱가드 유저id 세팅
        SP.AppguardSetting(userId);
    }

    // 계정 연결과 관련된 에러코드
    void CheckMappingError(int errorCode, Action ac)
    {
        SP.AddLog(Define.StrSB("Mapping failed. error is ", errorCode));

        if(errorCode != Toast.Gamebase.GamebaseErrorCode.AUTH_ADD_MAPPING_ALREADY_MAPPED_TO_OTHER_MEMBER)
            GMgr.Inst._CommonPopupMgr.OpenPopupTxt(CommonPopup.PopupState.BoxNone, errorCode.ToString());

        switch (errorCode)
        {
            case -100:
                {
                    // 강제맵핑 티켓이 없습니다.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.SOCKET_ERROR:
            case Toast.Gamebase.GamebaseErrorCode.SOCKET_RESPONSE_TIMEOUT:
                {
                    // 일시적인 네트워크 문제, 재시도 요청.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.AUTH_ADD_MAPPING_FAILED:
                {
                    // 매핑 추가에 실패했습니다.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.AUTH_ADD_MAPPING_ALREADY_MAPPED_TO_OTHER_MEMBER:
                {
                    // Mapping을 시도하는 IdP계정이 이미 다른 계정에 연동되어 있습니다.
                    // 강제로 연동을 해제하기 위해서는 해당 계정의 탈퇴나 Mapping 해제를 하거나, 다음과 같이
                    // ForcingMappingTicket을 획득 후, addMappingForcibly() 메소드를 이용하여 강제 매핑을 시도합니다.

                    // 이미 연동되어있는 계정으로 맵핑을 시도했다.(다른 계정에 맵핑이 연결되어 있는 상태)
                    // 유저에게 알리고 강제 맵핑을 하겠냐고 물어야겠다                        

                    //ShowPopupLinkForcibly(SP.ForcingMappingProvider);     // 강제 맵핑시도
                    //ShowPopupLinkForcibly(SP.ReConnectMappingProvider);     // 기존 계정으로 재로그인 시도

                    // 이미 연동된 계정이 있는데 그걸로 다시 접속하겠냐고 물어보는 팝업노출
                    // ...

                    GMgr.Inst._CommonPopupMgr.OpenPopup(CommonPopup.PopupState.BoxChoice, "setting_popup_message0003", (result) => 
                    {
                        switch(result)
                        {
                            case CommonPopup.PopupCallback.OK:
                                {
                                    //GameManager.Inst.startProcess.ChangeProcess(StartProcess.Process.CI);
                                    AddMappingGamebaseReConnect(requestProviderTemp, ac);
                                }
                                break;
                            case CommonPopup.PopupCallback.Cancel: break;
                        }
                    });
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.AUTH_ADD_MAPPING_ALREADY_HAS_SAME_IDP:
                {
                    // Mapping을 시도하는 IdP의 계정이 이미 추가되어 있습니다.
                    // Gamebase Mapping은 한 IdP당 하나의 계정만 연동 가능합니다.
                    // IdP 계정을 변경하려면 이미 연동중인 계정은 Mapping 해제를 해야 합니다.

                    // 예를들어 구글계정이 맵핑되어 있는데 또다른 구글계정을 추가맵핑 할 수 없다는 얘기
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.AUTH_ADD_MAPPING_INVALID_IDP_INFO:
                {
                    // IdP 정보가 유효하지 않습니다. (Console에 해당 IdP 정보가 없습니다.)
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.AUTH_ADD_MAPPING_CANNOT_ADD_GUEST_IDP:
                {
                    // 게스트 IdP로는 AddMapping이 불가능합니다.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.AUTH_ADD_MAPPING_FORCIBLY_NOT_EXIST_KEY:
                {
                    // 강제매핑키(ForcingMappingKey)가 존재하지 않습니다.
                    // ForcingMappingTicket을 다시 한번 확인해주세요.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.AUTH_ADD_MAPPING_FORCIBLY_ALREADY_USED_KEY:
                {
                    // 강제매핑키(ForcingMappingKey)가 이미 사용되었습니다.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.AUTH_ADD_MAPPING_FORCIBLY_EXPIRED_KEY:
                {
                    // 강제매핑키(ForcingMappingKey)의 유효기간이 만료되었습니다.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.AUTH_ADD_MAPPING_FORCIBLY_DIFFERENT_IDP:
                {
                    // 강제매핑키(ForcingMappingKey)가 다른 IdP에 사용되었습니다.
                    // 발급받은 ForcingMappingKey는 같은 IdP에 강제 매핑을 시도 하는데 사용됩니다.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.AUTH_ADD_MAPPING_FORCIBLY_DIFFERENT_AUTHKEY:
                {
                    // 강제매핑키(ForcingMappingKey)가 다른 계정에 사용되었습니다.
                    // 발급받은 ForcingMappingKey는 같은 IdP 및 계정에 강제 매핑을 시도 하는데 사용됩니다.
                }
                break;
            default: break;
        }
    }
    #endregion
}
