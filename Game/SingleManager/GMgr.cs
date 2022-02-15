using System;
using System.Collections.Generic;
using LetsBaseball.MasterData;
using FrameWork.UI;
using UnityEngine;

/// <summary>
/// Global Manager == GMgr 관리자..
/// : Singleton 을 줄이자..
/// : In/Out 상관없이 항시 상주..
/// </summary>
public class GMgr : SingleManager<GMgr>
{
    // Prefab
    public GameObject SPBehaviourPrefab = null;
    public GameObject soundMgrPrefab = null;
    public GameObject localizeMgrPrefab = null;
    public GameObject commonPopupPrefab = null;
    public GameObject uiMgrPrefab = null;

    // ScriptableObject

    public ResourceUIScriptableObject _ResourceUI = null;
    [HideInInspector] public LSBSoundResourceManager _SoundMgr;
    public ModelScriptableObject _ResourceModel = null;
    [HideInInspector] public LocalizeLoadManager _LocalizeMgr = null;

    [HideInInspector] public CommonPopupManager _CommonPopupMgr = null;
    [HideInInspector] public CommonPopupInfo _CommonPopupInfo = null;
    [HideInInspector] public SystemManager _SystemMgr = null;
    [HideInInspector] public UIResourceLoadManager _UIResourceMgr;
    [HideInInspector] public LocalNotification _LocalNotification = null;

    // State
    [HideInInspector] public ResourceState resourceState = ResourceState.Common;

    // load list
    private List<LoadAsync> loads = new List<LoadAsync>();

    [HideInInspector] public bool isInit = false;
    [HideInInspector] public bool istutorial = false;
    protected override void OnAwake()
    {
        dontDestroy = true;
        base.OnAwake();

        IsDone = false;

        if (SPBehaviourPrefab) Instantiate(SPBehaviourPrefab);
        if (commonPopupPrefab)
        {
            _CommonPopupMgr = Instantiate(commonPopupPrefab, transform).GetComponent<CommonPopupManager>();
            _CommonPopupMgr.Init();
        }

        // only add component..
        _CommonPopupInfo = gameObject.AddComponent<CommonPopupInfo>();
        _SystemMgr = gameObject.AddComponent<SystemManager>();
        _LocalNotification = gameObject.AddComponent<LocalNotification>();    // 로컬푸시
    }

    public void Init()
    {
        if (isInit) return;
        if (soundMgrPrefab) _SoundMgr = Instantiate(soundMgrPrefab, transform).GetComponent<LSBSoundResourceManager>();
        if (localizeMgrPrefab) _LocalizeMgr = Instantiate(localizeMgrPrefab, transform).GetComponent<LocalizeLoadManager>();
        if (uiMgrPrefab) _UIResourceMgr = Instantiate(uiMgrPrefab, transform).GetComponent<UIResourceLoadManager>();

        // resource
        loads.Add(_SoundMgr);
        loads.Add(_ResourceModel);
        loads.Add(_LocalizeMgr);
        loads.Add(_UIResourceMgr);
        OnLoad();

        isInit = true;
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
            GameManager.Inst.startProcess.PlayTitle(()=> { });
            _SoundMgr.InitAudioMixer();
        }
    }
    public bool IsDone { get; set; }    // 리소스 Init(로드) 완료

    public void LogInComplete(bool isSuccess)
    {
        //if(isSuccess)
        //    SocialPlayer.LoadAchievements();
    }
    public void LowShadow(bool active)
    {
        if (OMgr.Inst != null)
            OMgr.Inst._LobbyControlMgr.Lobby3DObjControl.SetLowShadow(active);
        else if (IMgr.Inst != null)
            IMgr.Inst.LowShadowSet(active);
    }
    public void CheckAchievement(string id)
    {
        //if (SocialPlayer.CanDoAchievement(id) == false)
        //    return;

        //switch(id)
        //{
        //    case GPGSIds.achievement_playing_the_first_game:
        //        {
        //            SocialPlayer.DoAchievement(GPGSIds.achievement_playing_the_first_game, 100.0f, (bool isSuccess) =>
        //            {
        //                if (isSuccess)
        //                {
        //                    TextHelper.ShowTMProText("achievement_playing_the_first_game is success.\n");
        //                }
        //                else
        //                {
        //                    TextHelper.ShowTMProText("achievement_playing_the_first_game is failed.\n");
        //                }
        //            });
        //        }
        //        break;
        //    case GPGSIds.achievement_taste_the_capsules:
        //        {
        //            SocialPlayer.DoAchievementIncrement(GPGSIds.achievement_taste_the_capsules, 1, (bool isSuccess) =>
        //            {
        //                if (isSuccess)
        //                {
        //                    TextHelper.ShowTMProText("achievement_taste_the_capsules is success.\n");

        //                    if (SocialPlayer.IsCompleteAchievement(GPGSIds.achievement_taste_the_capsules))  // ???????? ???? ???????? ???? ??????
        //                    {
        //                        // progress?? 0.0f???? ???????? ?????????? ??????????
        //                        SocialPlayer.DoAchievement(GPGSIds.achievement_strengthening_the_player, 0.0f, (bool isActive) =>
        //                        {
        //                            if (isActive)
        //                            {
        //                                TextHelper.ShowTMProText("achievement_strengthening_the_player is active.\n");
        //                            }
        //                            else
        //                            {
        //                                TextHelper.ShowTMProText("achievement_strengthening_the_player's activate failed.\n");
        //                            }
        //                        });
        //                    }
        //                }
        //                else
        //                {
        //                    TextHelper.ShowTMProText("achievement_taste_the_capsules is failed.\n");
        //                }
        //            });
        //        }
        //        break;
        //    case GPGSIds.achievement_strengthening_the_player:
        //        {
        //            SocialPlayer.DoAchievement(GPGSIds.achievement_strengthening_the_player, 100.0f, (bool isSuccess) =>
        //            {
        //                if (isSuccess)
        //                {
        //                    TextHelper.ShowTMProText("achievement_strengthening_the_player is success.\n");
        //                }
        //                else
        //                {
        //                    TextHelper.ShowTMProText("achievement_strengthening_the_player is failed.\n");
        //                }
        //            });
        //        }
        //        break;
        //    default:    break;
        //}
    }

    //void LoadResourceUIManager()
    //{
    //    LSBUIResourceManager.Manager.LoadSpriteManager(callbackLoadResourceManager);
    //    CustomLog.Log(WLogType.debug, "========== LoadResourceUIManager");
    //}

    //void callbackLoadResourceManager()
    //{
    //    PreLoadResourceUI();
    //    CustomLog.Log(WLogType.debug, "========== callbackLoadResourceManager");
    //}

    //void callbackPreLoad()
    //{
    //    LSBLocalize.Manager.LoadLocalizeData();
    //    LSBLocalize.Manager.SetLanguage(LocalizationLangList.Kr);

    //    CustomLog.Log(WLogType.debug, "========== callbackPreLoad");
    //}
    //void PreLoadResourceUI()
    //{
    //    LetsBaseball.MasterData.ImageResourceTypeList[] arrayType = new LetsBaseball.MasterData.ImageResourceTypeList[]{
    //        LetsBaseball.MasterData.ImageResourceTypeList.Bg,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Cover,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Gage,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Playercard,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Stat,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Skill,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Playerattribute,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Pack,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Iconcard,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Newshop,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Shopiconcat,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Shopiconruby,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Shotslot,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Itemmachine,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Itemglove,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Itembat,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Userprofilepicturegrade,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Nationflagbig,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Nationflagsmall,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Capsule,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Cardbtnbg,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Popupbtnbg,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Rankingslotbg,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Rankingnumber,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Leagueimagebig,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Leagueimagesmall,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Leagueimagetext,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Lobbyiconruby,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Emblemmark,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Emblembg,
    //        LetsBaseball.MasterData.ImageResourceTypeList.Mailicon,
    //    };

    //    //LSBUIResourceManager.Manager.PreLoadAtlas(arrayType, callbackPreLoad);
    //}

    #region _test
#if  UNITY_EDITOR
    //private void Update()
    //{
    //}

#endif
    #endregion _test

    #region Mobile Notifications    
    protected void OnApplicationFocus(bool hasFocus)
    {
        if (_LocalNotification == null)
            return;

        if (_LocalNotification.Initialized == false)
            return;

        if(hasFocus == false)   
        {
            // Backgrounding
            CheckCapsuleNotifications();
        }

        _LocalNotification.OnFocus(hasFocus);
    }

    void CheckCapsuleNotifications()
    {
        CapsuleExtention capsuleExtention = ServerInfo.capsule;
        if(capsuleExtention != null)
        {
            double timeRemining = 0;
            int count = capsuleExtention.CapsuleSlotsServer.Count;

            for (int i = 0; i < count; i++)
            {
                timeRemining = (int)capsuleExtention.CapsuleSlotsServer[i].GetLimitTime();

                if (timeRemining > 0 && capsuleExtention.CapsuleSlotsServer[i].capsuleState == CapsuleOpenInfo.CapsuleState.Openning)
                {
                    // Title, Decsription 은 알림에 따른 로컬라이징 텍스트로 대체해줘라.
                    // 알림에 따른 아이콘 이름을 입력해줘라(아이콘은 ProjectSettings에 mobile notification의 Android탭에 빌드인 등록
                    // 아이콘은 iOS에서는 사용되지 않음
                    _LocalNotification.SendNotification(i,
                                                        "Capsule",
                                                        "Open the capsule.",
                                                        TimeSpan.FromSeconds(timeRemining),
                                                        NotificationChannelType.Item.ToString(),
                                                        "small_capsule",
                                                        "large_capsule");
                }
            }
        }
    }

    public void CancelNotification(int id)
    {
        if (_LocalNotification)
        {
            _LocalNotification.CancelNotification(id);
        }
    }

    //protected void OnApplicationFocus(bool hasFocus)
    //{
    //    if (_NotificationsManager == null)
    //        return;

    //    if(hasFocus)
    //    {
    //        #region Gacha capsule            
    //        GachaExtention gachaExtention = ServerInfo.gacha;
    //        double timeRemining = 0;
    //        int count = gachaExtention.openInfo.Count;
    //        DateTime deliveryTime = new DateTime();

    //        for (int i = 0; i < count; i++)
    //        {
    //            timeRemining = (int)gachaExtention.openInfo[i].GetLimitTime();
    //            if(timeRemining > 0 && gachaExtention.openInfo[i].openState == CapsuleOpenInfo.OpenState.Openning)
    //            {
    //                deliveryTime = DateTime.Now.ToLocalTime() + TimeSpan.FromSeconds(timeRemining);

    //                _NotificationsManager.SendNotification(i,
    //                                                       "Capsule",
    //                                                       "Open the capsule.",
    //                                                       deliveryTime,
    //                                                       NotificationChannelType.Item,
    //                                                       null,
    //                                                       "large_capsule");
    //            }
    //        }
    //        #endregion
    //    }

    //    _NotificationsManager.OnFocus(hasFocus);
    //}

    //// id : 다른 알림과 중복되지 않게
    //// title : 알림제목
    //// body : 내용
    //// deliveryTime : 노티시간
    //// type : 채널타입
    //// smallIcon, largeIcon : 아이콘 네이밍
    //public void SetNotification(int id, string title, string body, DateTime deliveryTime, NotificationChannelType type, string smallIcon = null, string largeIcon = null)
    //{
    //    if (_NotificationsManager == null)
    //        return;

    //    _NotificationsManager.SendNotification(id,
    //                                         title,
    //                                         body,
    //                                         deliveryTime,
    //                                         type,
    //                                         smallIcon,
    //                                         largeIcon);
    //}

    //public void CancelNotification(int id)
    //{
    //    if (_NotificationsManager == null)
    //        return;

    //    _NotificationsManager.CancelNotification(id);
    //}
    #endregion
}
