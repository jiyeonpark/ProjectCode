using System.Collections;
using System.Collections.Generic;
using LetsBaseball.MasterData;
using UnityEngine;
using Cysharp.Threading.Tasks;
using WCS.Network;
#region _GameBase Pay
using ServicePlatform;
using UnityEngine.Networking;
using System;
using LitJson;
#endregion _GameBase Pay

/// <summary>
/// OutGame Manager == OMgr 관리자..
/// : Singleton 을 줄이자..
/// </summary>
public class OMgr : SingleManager<OMgr>
{
    // Prefab
    public GameObject soundMgrPrefab = null;

    // ScriptableObject
    public ResourceUIScriptableObject _ResourceUI = null;
    public ModelScriptableObject _ResourceModel = null;

    public GameObject modelStorage = null;

    [HideInInspector] public OutGameManager _OutGameMgr;
    [HideInInspector] public OutGameUIManager _OutGameUIMgr;
    [HideInInspector] public LobbyControl _LobbyControlMgr;
    [HideInInspector] public LSBSoundResourceManager _SoundMgr;


    // State
    [HideInInspector] public ResourceState resourceState = ResourceState.OutGame;

    // load list
    private List<LoadAsync> loads = new List<LoadAsync>();

    protected override void OnAwake()
    {
        IsDone = false;

        if (soundMgrPrefab) _SoundMgr = Instantiate(soundMgrPrefab, transform).GetComponent<LSBSoundResourceManager>();

        // only add component..
        _OutGameMgr = gameObject.AddComponent<OutGameManager>();

        // find..
        _OutGameUIMgr = GameObject.Find("MainCanvas").GetComponent<OutGameUIManager>();
        _LobbyControlMgr = GameObject.Find("UIControl").GetComponent<LobbyControl>();
        _LobbyControlMgr.InitUIRef();
        _ResourceModel.gameType = ModelScriptableObject.GameType.OutGame;

        // resource
        loads.Add(_SoundMgr);
        loads.Add(_ResourceModel);
        OnLoad();
    }

    protected override void OnStart()
    {
        base.OnStart();

    }

    void OnLoad()
    {
        for (int i = 0; i < loads.Count; i++)
        {
            loads[i].Init(resourceState, () => { OnDone(); });
        }
    }

    void OnDone()
    {
        bool isDone = true;
        for (int i = 0; i < loads.Count; i++)
        {
            if (!loads[i].IsDone()) isDone = false;
        }

        // all completed
        if (isDone && !IsDone)
        {
            IsDone = true;
            LobbyFlow();
        }
    }
    async UniTask LobbyFlow()
    {
        await UniTask.Yield();

        SetPayEvent();

        // 모든 앞단 로딩 완료!
        //로비화면이 준비가 되면 이벤트를 진행한다.
        //이벤트는 튜토리얼, 공지사항, 출석보상 등이 있다, 나중에 더 추가.
        await Tutorial();
        await Notice();
        await Attendance();
    }

    /// <summary>
    /// 튜토리얼
    /// 튜토리얼 체크.
    /// </summary>
    /// <returns></returns>
    async UniTask Tutorial()
    {
        await UniTask.Yield();
        if (true == ServerInfo.tutorialmgr.IsTutorial())
        {
            CustomLog.Log(WLogType.donghee, $"========== Start Tutorial");

            _LobbyControlMgr.OpenPage();

            await CheckTestTutorial();

            // UI 준비완료시 화면전환!
            if (GameManager.Inst.startProcess) GameManager.Inst.startProcess.CloseAllProcess();
            if (UILoading.IsCreate) UILoading.Inst.SetActive(false);

            ServerInfo.tutorialmgr.StartTutorial();

            await UniTask.WaitUntil(() => ServerInfo.tutorialmgr.IsTutorial() == false);
        }else
        {
            await StartLobby();

            // UI 준비완료시 화면전환!
            if (GameManager.Inst.startProcess) GameManager.Inst.startProcess.CloseAllProcess();
            if (UILoading.IsCreate) UILoading.Inst.SetActive(false);
        }
        CustomLog.Log(WLogType.donghee, $"========== End Tutorial");


    }

    void SetPayEvent()
    {
        acPayRequestPurchaseItemSuccess = CBPayRequestPurchaseItemSuccess;
        acPayRequestPurchaseItemFail = CBPayRequestPurchaseItemFail;
    }
    #region _test
    bool waitResp = false;
    async UniTask CheckTestTutorial()
    {
        if (GameManager.Inst.serverVersion == LetsBaseball.Network.Http.ServerVersion.none) return;

        //정상적인 방법으로 튜토리얼을 시작함.
        if (TutorialMgr.TestStep.None == ServerInfo.tutorialmgr.testStep) return;

        //인게임을 스킵하고 아웃게임 튜토리얼 시작.
        //서버 값을 새로 세팅해야함.
        //ReStart 일경우는 인게임으로 가기 때문에 여기서 체크되면 안된다.
        if (TutorialMgr.TestStep.ReStartOutGame == ServerInfo.tutorialmgr.testStep)
        {
            //캡슐 슬롯이 비어있어야 한다.
            if (0 == ServerInfo.capsule.CapsuleSlotsServer.Count)
            {
                waitResp = true;
                LobbyServer.Inst.LobbyPropertySet(wce_property_type.tutorial_step, (int)TutorialMgr.TutorialSeverStep.Step01).Callback(
                    success: (response) =>
                    {
                        waitResp = false;
                        if (0 < response.reward_items.Count)
                        {
                            ServerInfo.tutorialmgr.ResetTestTutorial();

                            wcs_capsule capsule = new wcs_capsule(response.reward_items[0]);
                            ServerInfo.capsule.AddServerCapsule(capsule);
                            OMgr.Inst._LobbyControlMgr.StartDefaultLobby();
                            CustomLog.Log(WLogType.donghee, $"========== CheckTestTutorial [{response.reward_items.Count}]");
                        }
                    },
                    fail: (err) =>
                    {
                        waitResp = false;
                        CustomLog.Log(WLogType.debug, "CheckTestTutorial fail");
                    }
                 );
            }
        }
        //Skip 일경우 서버 값을 종료값으로 보내준다.
        else if (TutorialMgr.TestStep.Skip== ServerInfo.tutorialmgr.testStep)
        {
            waitResp = true;
            LobbyServer.Inst.LobbyPropertySet(wce_property_type.tutorial_step,(int)TutorialMgr.TutorialSeverStep.StepEnd).Callback(
                success: (response) =>
                {
                    waitResp = false;
                    ServerInfo.tutorialmgr.ResetTestTutorial();
                },
                fail: (err) =>
                {
                    waitResp = false;
                    CustomLog.Log(WLogType.debug, "CheckTestTutorial fail");
                }
             );

        }

        await UniTask.WaitUntil(()=>waitResp == false);
    }
    //async UniTask CheckTestTutorial()
    //{
    //    if (true == ServerInfo.tutorialmgr.skipTutorial) return;
    //    if (true == ServerInfo.tutorialmgr.testTutorial && true == ServerInfo.tutorialmgr.testTutorialReset )
    //    {
    //        //튜토리얼 세팅 리셋.
    //    }
    //    if (true == ServerInfo.tutorialmgr.testTutorial && false == ServerInfo.tutorialmgr.testTutorialReset)
    //    {
    //        if (0 == ServerInfo.capsule.CapsuleStats.Count)
    //        {
    //            LobbyServer.Inst.LobbyPropertySet(wce_property_type.tutorial_step, 1).Callback(
    //                success: (response) =>
    //                {
    //                    if (0 < response.reward_items.Count)
    //                    {
    //                        wcs_capsule capsule = new wcs_capsule(response.reward_items[0]);
    //                        ServerInfo.capsule.AddServerCapsule(capsule);
    //                        OMgr.Inst._LobbyControlMgr.StartDefaultLobby();
    //                        CustomLog.Log(WLogType.donghee, $"========== CheckTestTutorial [{response.reward_items.Count}]");
    //                    }
    //                },
    //                fail: (err) =>
    //                {
    //                    CustomLog.Log(WLogType.debug, "CheckTestTutorial fail");
    //                }
    //             );
    //        }
    //        else
    //        {
    //            //진행 상태를 체크한다.
    //        }

    //    }
    //}

#if  UNITY_EDITOR
    private void Update()
    {
        if (true == Input.GetKeyUp(KeyCode.W) && resourceState == ResourceState.OutGame)
        {
            LobbyServer.Inst.LobbyPropertySet(wce_property_type.tutorial_step, 1).Callback(
                success: (response) =>
                {
                    if (0 < response.reward_items.Count)
                    {
                        wcs_capsule capsule = new wcs_capsule(response.reward_items[0]);
                        ServerInfo.capsule.AddServerCapsule(capsule);
                        OMgr.Inst._LobbyControlMgr.StartDefaultLobby();
                        CustomLog.Log(WLogType.donghee, $"========== CheckTestTutorial [{response.reward_items.Count}]");
                    }
                },
                fail: (err) =>
                {
                    CustomLog.Log(WLogType.debug, "CheckTestTutorial fail");
                }
             );

            //LobbyServer.Inst.LobbyPropertySet(wce_property_type.tutorial_step, 0).Callback(
            //    success: (response) =>
            //    {

            //    },
            //    fail: (err) =>
            //    {
            //        CustomLog.Log(WLogType.debug, "CheckTestTutorial fail");
            //    }
            // );

        }
    }
#endif

    #endregion _test


    /// <summary>
    /// 공지
    /// </summary>
    async UniTask Notice()
    {
        await UniTask.Yield();

    }

    async UniTask Attendance()
    {
        await UniTask.Yield();
    }

    async UniTask StartLobby()
    {
        await UniTask.Yield();
        _LobbyControlMgr.OpenPage();
    }

    public bool IsDone { get; set; }    // 리소스 Init(로드) 완료

    #region _GameBase Pay
    //SPGUITester 내용 복사.
    IEnumerator coroutineGamebaseConsume = null;

    //우선 모든 callback 은 밖으로
    public Action acPayRequestItemListPurchasableSuccess = null;
    public Action acPayRequestItemListPurchasableFail = null;
    public Action acPayRequestItemListOfNotConsumedSuccess = null;
    public Action acPayRequestItemListOfNotConsumedFail = null;
    public Action acPayConsumeItemsToGameServerSuccess = null;
    public Action acPayConsumeItemsToGameServerFail = null;
    public Action acPayConsumeItemsToGameBaseSuccess = null;
    public Action acPayConsumeItemsToGameBaseFail = null;
    public Action acPayConsumeItemsSuccess = null;
    public Action acPayConsumeItemsFail = null;
    public Action acPayConsumeItemsCompleteSuccess = null;
    public Action acPayConsumeItemsCompleteFail = null;
    public Action acPayRequestPurchaseItemSuccess = null;
    public Action acPayRequestPurchaseItemFail = null;

    public void CBPayRequestPurchaseItemSuccess()
    {
        CustomLog.Log(WLogType.donghee, $"========== CBPayRequestPurchaseItemSuccess");
    }
    public void CBPayRequestPurchaseItemFail()
    {
        CustomLog.Log(WLogType.donghee, $"========== CBPayRequestPurchaseItemFail");
    }

    /// <summary>
    /// 현재 게이베이스에 등록된 인앱 상품목록
    /// isSuccess : 성공여부
    /// purchasableItems : 아이템 리스트
    /// error : 에러코드
    /// </summary>
    /// <param name="cbSuccess"></param>
    /// <param name="cbFail"></param>
    public void RequestItemListPurchasable()
    {
        SP.RequestItemListPurchasable((isSuccess, purchasableItems, error) =>
        {
            if (isSuccess)
            {
                if (SP.purchasableItems.Count == 0)
                {
                    //AddLog("There are no items available for purchase. Register your product in the TOAST Console");
                }
                else
                {
                    for (int i = 0; i < SP.purchasableItems.Count; i++)
                    {
                        //AddLog(Define.StrSB("Id: ", SP.purchasableItems[i].gamebaseProductId, " Price: ", SP.purchasableItems[i].price.ToString(), " Name: ", SP.purchasableItems[i].name));
                        //AddLog(Define.StrSB("LocalPrice: ", SP.purchasableItems[i].localizedPrice));
                        //AddLog(Define.StrSB("LocalTitle: ", SP.purchasableItems[i].localizedTitle));
                        //AddLog(Define.StrSB("LocalDesc: ", SP.purchasableItems[i].localizedDescription));
                    }

                    if (null != acPayRequestItemListPurchasableSuccess) acPayRequestItemListPurchasableSuccess.Invoke();
                }
            }
            else
            {
                CheckIAPError(error);
                if (null != acPayRequestItemListPurchasableFail) acPayRequestItemListPurchasableFail.Invoke();
            }
        });
    }

    /// <summary>
    /// 미소비(미처리)된 아이템들 조회
    /// [ 호출 타이밍 ]
    /// 신규 결제시(isSilent : false)
    /// 로그인 완료후(isSilent : true)
    /// 상점 등 결제와 관련된 화면 전환시(isSilent : true)
    /// </summary>
    /// <param name="isSilent"></param>
    public void RequestItemListOfNotConsumed(bool isSilent = false)
    {
        if (SP.GetLastLoggedInProvider() == "guest")
            return;

        SP.RequestItemListOfNotConsumed((isSuccess, error) =>
        {
            if (isSuccess)
            {
                if (null != acPayRequestItemListOfNotConsumedSuccess) acPayRequestItemListOfNotConsumedSuccess.Invoke();
                if (error == -1 || error == 0)
                {
                    // 처리해야할 아이템이 없다.(이슈없음)                        
                }
                else
                {
                    SP.IsPurchaseLock = true;

                    // 1. 서버에 아이템 처리 요청
                    ConsumeItemsToGameServer(isSilent);

                    // 1번 처리 끝나면 게임베이스에 아이템 처리를 해주어야 한다.
                    // ConsumeItemsToGameBase() : 현재 구문에서 호출해주면 안됨
                }
            }
            else
            {
                CheckIAPError(error);
                if (null != acPayRequestItemListOfNotConsumedFail) acPayRequestItemListOfNotConsumedFail.Invoke();
            }
        });
    }

    /// <summary>
    /// 서버에 아이템 처리를 요청하자(미소비 아이템들의 존재 때문에 n개 일 수 있다)
    /// </summary>
    /// <param name="isSilent"></param>
    public void ConsumeItemsToGameServer(bool isSilent = false)
    {

        if (null != acPayConsumeItemsToGameServerSuccess) acPayConsumeItemsToGameServerSuccess.Invoke();
        // 서버에 아이템 처리를 요청하는 코드나 함수를 작업하세요.
        // 요청해야할 아이템 정보(list) : SP.consumeItems

        //SP.consumeItems.Find(x=>x.gamebaseProductId)

        // 서버와의 연동도 콜백 딜레이가 있을테니 Action이던 Task던 별도의 펑션으로 만들어도 됩니다.
        // 중요한건 서버와의 처리가 끝나고 아래 ConsumeItemsToGameBase() 함수를 호출해주면 됩니다.
        //ConsumeItemsToGameBase(isSilent);     // SPGUI test call.
    }

    /// <summary>
    /// 서버와의 처리가 다 끝난 후 호출해주는 함수
    /// </summary>
    /// <param name="isSilent"></param>
    public void ConsumeItemsToGameBase(bool isSilent = false)
    {
        if (null != acPayConsumeItemsToGameBaseSuccess) acPayConsumeItemsToGameBaseSuccess.Invoke();

        if (coroutineGamebaseConsume != null)
        {
            StopCoroutine(coroutineGamebaseConsume);
            coroutineGamebaseConsume = null;
        }
        coroutineGamebaseConsume = ConsumeItems(isSilent);
        StartCoroutine(coroutineGamebaseConsume);
    }

    IEnumerator ConsumeItems(bool isSilent = false)
    {
        int itemCount = SP.consumeItems.Count;

        Dictionary<string, string> formFields = new Dictionary<string, string>();
        formFields.Clear();
        string paymenSeq = string.Empty;
        string pruchaseToken = string.Empty;
        //string Uri = Define.StrSB("https://api-gamebase.cloud.toast.com/tcgb-inapp/v1.3/apps/", SP.AppID, "/consume"); //"https://api-gamebase.cloud.toast.com/tcgb-inapp/v1.3/apps/YHE2WOMZ/consume";
        string Uri = Define.StrSB(SP.ConsumeItemUrl, SP.AppID);
        string SECRETKEY = SP.SecretKey; //"lzonrTYx";

        for (int i = 0; i < itemCount; i++)
        {
            paymenSeq = SP.consumeItems[i].paymentSeq;
            pruchaseToken = SP.consumeItems[i].purchaseToken;

            formFields.Clear();
            formFields.Add("paymentSeq", paymenSeq);
            formFields.Add("accessToken", pruchaseToken);

            string jsonData = JsonMapper.ToJson(formFields);
            byte[] jsonToByte = new System.Text.UTF8Encoding().GetBytes(jsonData);

            UnityWebRequest request = UnityWebRequest.Post(Uri, jsonData);
            request.uploadHandler = new UploadHandlerRaw(jsonToByte);
            request.SetRequestHeader("Content-Type", "application/json");
            //request.SetRequestHeader("X-TCGB-Transaction-Id", "2021083054844544");
            request.SetRequestHeader("X-Secret-Key", SECRETKEY);
            request.timeout = 5;

            yield return UnityCompatibility.UnityWebRequest.Send(request);

            if (request.isDone == true)
            {
                if (request.responseCode != 200)
                {
                    SP.AddLog("request.responseCode != 200");
                }
                else if (UnityCompatibility.UnityWebRequest.IsError(request) == true)
                {
                    SP.AddLog("UnityCompatibility.UnityWebRequest.IsError(request) == true");
                }
                else if (string.IsNullOrEmpty(request.downloadHandler.text) == true)
                {
                    SP.AddLog("string.IsNullOrEmpty(request.downloadHandler.text) == true");
                }
                else
                {
                    //byte[] result = request.downloadHandler.data;
                    //string resultString = System.Text.Encoding.UTF8.GetString(result);

                    //AddLog(Define.StrSB("Consume Success response : ", request.downloadHandler.text));
                }
            }

            request.Dispose();

            yield return null;
        }

        SP.IsPurchaseLock = false;

        if (null != acPayConsumeItemsSuccess) acPayConsumeItemsSuccess.Invoke();
        //AddLog("ConsumeItems completed.");
        ConsumeItemsComplete(isSilent);
    }

    /// <summary>
    /// 모든 처리가 끝났다
    /// </summary>
    /// <param name="isSilent"></param>
    public void ConsumeItemsComplete(bool isSilent = false)
    {
        if (null != acPayConsumeItemsCompleteSuccess) acPayConsumeItemsCompleteSuccess.Invoke();
        // UI정보 노출을 위해 SP.consumeItems 정보 참조

        if (isSilent == false)
        {
            // 아이템을 구매하였습니다. 팝업 노출
        }
        else
        {
            // 그냥 조용히...
            // 아니면 서버에 로그라도 남기던지?
        }

        SP.ResetConsumeItems();
    }

    /// <summary>
    /// 아이템 구매요청(게임베이스 상품id)
    /// </summary>
    /// <param name="productId"></param>
    public void RequestPurchaseItem(string productId)
    {
        if (SP.GetLastLoggedInProvider() == "guest")
            return;

        SP.PurchaseItem(productId, (isSuccess, purchasableReceipt, error) =>
        {
            if (isSuccess)
            {
                //AddLog(Define.StrSB("[IAP] RequestPurchaseItem is succeeded : ", purchasableReceipt.gamebaseProductId));
                if (null != acPayRequestPurchaseItemSuccess) acPayRequestPurchaseItemSuccess.Invoke();
                RequestItemListOfNotConsumed();
            }
            else
            {
                if (null != acPayRequestPurchaseItemFail) acPayRequestPurchaseItemFail.Invoke();
                CheckIAPError(error);
            }
        });
    }

    public void CheckIAPError(int errorCode)
    {
        SP.AddLog(Define.StrSB("IAPError : ", errorCode));

        SP.IsPurchaseLock = false;

        switch (errorCode)
        {
            case Toast.Gamebase.GamebaseErrorCode.PURCHASE_NOT_INITIALIZED:
                {
                    // Purchase 모듈이 초기화되지 않았습니다.
                    // gamebase-adapter-purchase-IAP 모듈을 프로젝트에 추가했는지 확인해 주세요.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.PURCHASE_USER_CANCELED:
                {
                    // 게임 유저가 아이템 구매를 취소하였습니다
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.PURCHASE_NOT_FINISHED_PREVIOUS_PURCHASING:
                {
                    // 구매 로직이 아직 완료되지 않은 상태에서 API가 호출되었습니다.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.PURCHASE_NOT_ENOUGH_CASH:
                {
                    // 해당 스토어의 캐시가 부족해 결제할 수 없습니다.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.PURCHASE_INACTIVE_PRODUCT_ID:     // <NATIVE ONLY>
                {
                    // 해당 상품이 활성화 상태가 아닙니다
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.PURCHASE_NOT_EXIST_PRODUCT_ID:    // <NATIVE ONLY>
                {
                    // 존재하지 않는 GamebaseProductID 로 결제를 요청하였습니다.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.PURCHASE_LIMIT_EXCEEDED:          // <NATIVE ONLY>
                {
                    // 월 구매 한도를 초과했습니다.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.PURCHASE_NOT_SUPPORTED_MARKET:
                {
                    // 지원하지 않는 스토어입니다.
                    // 선택 가능한 스토어는 AS(App Store), GG(Google), ONESTORE, GALAXY 입니다.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.PURCHASE_UNKNOWN_ERROR:
                {
                    // 정의되지 않은 구매 오류입니다.
                    // 전체 로그를 고객 센터에 올려 주시면 가능한 한 빠르게 답변 드리겠습니다.
                }
                break;
            case Toast.Gamebase.GamebaseErrorCode.PURCHASE_EXTERNAL_LIBRARY_ERROR:
                {
                    // IAP 라이브러리 오류입니다.
                    // DetailCode를 확인하세요.
                    // 여기는 들어오지 않고 하위 case문들로 들어올것이다.
                }
                break;
            #region External Common Error
            case 50000:
                {
                    // 초기화 되지 않았습니다.
                }
                break;
            case 50001:
                {
                    // 지원하지 않는 기능입니다.
                }
                break;
            case 50002:
                {
                    // 지원하지 않는 스토어 코드입니다.
                }
                break;
            case 50003:
                {
                    // 사용할 수 없는 상품입니다.
                }
                break;
            case 50004:
                {
                    // 이미 소유중인 상품입니다.
                }
                break;
            case 50006:
                {
                    // 사용자 아이디가 잘못되었습니다.
                }
                break;
            case 50007:
                {
                    // 사용자가 결제를 취소하였습니다.
                }
                break;
            case 50009:
                {
                    // 영수증 검증에 실패했습니다.
                }
                break;
            case 50011:
                {
                    // 구독 갱신이 실패했습니다.
                }
                break;
            case 50015:
                {
                    // 소유하고 있지 않은 상품 입니다.
                }
                break;
            case 50103:
                {
                    // 이미 소비된 상품 입니다.
                }
                break;
            case 50104:
                {
                    // 이미 환불된 상품 입니다.
                }
                break;
            case 50105:
                {
                    // 구매 한도를 초과했습니다.
                }
                break;
            case 59999:
                {
                    // 알 수 없는 에러입니다. 에러 메시지를 확인해 주세요.
                }
                break;
            #endregion
            #region External Server Error
            case 10000:
                {
                    // 잘못된 요청입니다.
                }
                break;
            case 10002:
                {
                    // 네트워크가 연결되지 않았습니다.
                }
                break;
            case 10003:
                {
                    // 서버 응답이 실패했습니다.
                }
                break;
            case 10004:
                {
                    // 타임아웃이 발생했습니다.
                }
                break;
            case 10005:
                {
                    // 유효하지 않은 서버 응답값입니다.
                }
                break;
            case 10010:
                {
                    // 활성화 되지 않은 앱입니다.
                }
                break;
            #endregion
            #region External App Store Error
            case 50005:
                {
                    // 이미 진행중인 요청이 있습니다.
                }
                break;
            case 50008:
                {
                    // 스토어에서 결제가 실패했습니다.
                }
                break;
            case 50010:
                {
                    // 구매상태 변경에 실패했습니다.
                }
                break;
            case 50012:
                {
                    // 환불로 인해 구매를 진행할 수 없습니다.
                }
                break;
            case 50013:
                {
                    // 복원에 실패했습니다.
                }
                break;
            case 50014:
                {
                    // 구매 진행 불가 상태입니다.(e.g. 앱 내 구입 제한 설정)
                }
                break;
            #endregion
            default: break;
        }
    }


    #endregion _GameBase Pay

}
