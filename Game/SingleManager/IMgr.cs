using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// InGame Manager == IMgr 관리자..
/// : Singleton 을 줄이자..
/// </summary>
public class IMgr : SingleManager<IMgr>
{
    // Prefab
    public GameObject ingameMgrPrefab = null;
    public GameObject cameraMgrPrefab = null;
    public GameObject ballMgrPrefab = null;
    public GameObject soundMgrPrefab = null;
    public GameObject particleMgrPrefab = null;
    public GameObject uniformMgrPrefab = null;
    public GameObject modelMgrPrefab = null;
    public GameObject timeLineCamMgrPrefab = null;

    // ScriptableObject
    public ResourceUIScriptableObject _ResourceUI = null;
    public ModelScriptableObject _ResourceModel = null;

    public GameObject modelStorage = null;

    [HideInInspector] public InGameManager _InGameMgr;
    [HideInInspector] public CameraManager _CameraMgr;
    [HideInInspector] public BallManager _BallMgr;
    [HideInInspector] public LSBSoundResourceManager _SoundMgr;
    [HideInInspector] public ParticleManager _ParticleMgr;

    [HideInInspector] public XmlManager _XmlMgr;
    [HideInInspector] public ScoreManager _ScoreMgr;
    [HideInInspector] public DefenceManager _DefenceMgr;
    [HideInInspector] public StadiumManager _StadiumMgr;
    [HideInInspector] public RunnerManager _RunnerMgr;
    [HideInInspector] public UniformManager _UniformMgr;
    [HideInInspector] public TimeLineCamMng _TimeLineCamMgr;
    [HideInInspector] public InGameUIManager _InGameUIMgr;
    [HideInInspector] public InGameUIControl _InGameUI;
    [HideInInspector] public ModelManager _ModelMgr;
    [HideInInspector] public SkillManager _SkillMgr;

    [HideInInspector] public TutorialFlow _tFlow;
    [HideInInspector] public IngameFlow _Flow;
    [HideInInspector] public IngameConnect _Connect;
    [HideInInspector] public StartFlow _StartFlow;
    [HideInInspector] public SkillFlow _SkillFlow;
    [HideInInspector] public ResultFlow _ResultFlow;
    [HideInInspector] public ResultSkillFlow _ResultSkillFlow;
    [HideInInspector] public Skill3DDirecting _SkillDirecting;

    // State
    [HideInInspector] public ResourceState resourceState = ResourceState.InGame;

    // load list
    private List<LoadAsync> loads = new List<LoadAsync>();

    protected override void OnAwake()
    {
        IsDone = false;

        if (modelMgrPrefab) _ModelMgr = Instantiate(modelMgrPrefab, transform).GetComponent<ModelManager>();
        if (ingameMgrPrefab) _InGameMgr = Instantiate(ingameMgrPrefab, transform).GetComponent<InGameManager>();
        if (cameraMgrPrefab) _CameraMgr = Instantiate(cameraMgrPrefab, transform).GetComponent<CameraManager>();
        if (ballMgrPrefab) _BallMgr = Instantiate(ballMgrPrefab, transform).GetComponent<BallManager>();
        if (soundMgrPrefab) _SoundMgr = Instantiate(soundMgrPrefab, transform).GetComponent<LSBSoundResourceManager>();
        if (particleMgrPrefab) _ParticleMgr = Instantiate(particleMgrPrefab, transform).GetComponent<ParticleManager>();
        if (uniformMgrPrefab) _UniformMgr = Instantiate(uniformMgrPrefab, transform).GetComponent<UniformManager>();
        if (timeLineCamMgrPrefab) _TimeLineCamMgr = Instantiate(timeLineCamMgrPrefab, transform).GetComponent<TimeLineCamMng>();

        if (GMgr.Inst.istutorial)
        {
            ModelSetting();
            _tFlow = gameObject.AddComponent<TutorialFlow>();
        }
        else
            _Flow = gameObject.AddComponent<IngameFlow>();
        _Connect = gameObject.AddComponent<IngameConnect>();
        _StartFlow = gameObject.AddComponent<StartFlow>();
        _SkillFlow = gameObject.AddComponent<SkillFlow>();
        _ResultFlow = gameObject.AddComponent<ResultFlow>();
        _ResultSkillFlow = gameObject.AddComponent<ResultSkillFlow>();
        if (XmlManager.sInstance == null)
            _XmlMgr = gameObject.AddComponent<XmlManager>();
        else
            _XmlMgr = XmlManager.sInstance;
        _ScoreMgr = gameObject.AddComponent<ScoreManager>();
        _StadiumMgr = gameObject.AddComponent<StadiumManager>();
        _RunnerMgr = gameObject.AddComponent<RunnerManager>();
        _DefenceMgr = gameObject.AddComponent<DefenceManager>();
        _SkillMgr = gameObject.AddComponent<SkillManager>();
        _SkillDirecting = gameObject.AddComponent<Skill3DDirecting>();

        // only add component..
        gameObject.AddComponent<RayCheck>();

        // find..
        _InGameUI = GameObject.Find("UIControl").GetComponent<InGameUIControl>();
        _InGameUI.InitUIRef();

        // resource
        loads.Add(_ResourceModel);
        loads.Add(_SoundMgr);
        loads.Add(_ParticleMgr);
        OnLoad();

        // code..
        GameManager.Inst.IsFirstEnterLobby = false; // 로비 최초 진입상태 : false변경(임시)
    }

    void OnLoad()
    {
        for (int i = 0; i<loads.Count; i++)
        {
            loads[i].Init(resourceState, ()=> { OnDone(); });
        }
    }

    void OnDone()
    {
        bool isDone = true;
        for (int i = 0; i < loads.Count; i++)
        {
            if(!loads[i].IsDone()) isDone = false;
        }

        // all completed
        if (isDone && !IsDone)
        {
            // 완료
            IsDone = true;
            GameEvent.OnLoadModelEvent();   // 모델로딩 시작

            if (GameManager.Inst.startProcess)
            {
                this.After(1f, () => { GameManager.Inst.startProcess.CloseAllProcess(); });
            }

            _SoundMgr.PlaySound(LetsBaseball.MasterData.SoundClip.IngameBgm);
        }
    }
    public bool IsDone { get; set; }    // 리소스 Init(로드) 완료

    void ModelSetting()
    {
        PlayerCardInfo player = new PlayerCardInfo();
        player.Set(1);
        ItemCardInfo item = new ItemCardInfo();
        item.Set(1);
        GInfo.enemyTeam.Set(GInfo.enemyTeam.userInfo.Reset(), player.Get(), item.Get());
        player = new PlayerCardInfo();
        player.Set(1);
        item = new ItemCardInfo();
        item.Set(1);
        GInfo.myTeam.Set(ServerInfo.user.Info, player.Get(), item.Get());

        //타자 3
        //투수 2
        //수비수 5
    }
    //그림자 세팅
    public void LowShadowSet(bool active)
    {
        //타자
        GInfo.myTeam.teamFuc.swing.OnLowShadow(active);
        GInfo.enemyTeam.teamFuc.swing.OnLowShadow(active);
        //투수
        GInfo.myTeam.teamFuc.pitching.OnLowShadow(active);
        GInfo.enemyTeam.teamFuc.pitching.OnLowShadow(active);
        //주자
        for (int i = 0; i < GInfo.myTeam.teamFuc.runners.Length; i++)
        {
            GInfo.myTeam.teamFuc.runners[i].OnLowShadow(active);
            GInfo.enemyTeam.teamFuc.runners[i].OnLowShadow(active);
        }
        //수비수
        for (int i = 0; i < GInfo.myTeam.teamFuc.defenders.Length; i++)
        {
            GInfo.myTeam.teamFuc.defenders[i].OnLowShadow(active);
            GInfo.enemyTeam.teamFuc.defenders[i].OnLowShadow(active);
        }
        _BallMgr.ballMove.shadowActive = active;
    }
}
