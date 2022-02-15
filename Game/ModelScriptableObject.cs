using System.Collections;
using System.Collections.Generic;
//using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using UnityEngine.Events;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ModelScriptableObject", order = 1)]

public class ModelScriptableObject : LoadScriptable
{
    public class ModelClass
    {
        public enum skinType
        {
            Yellow,
            White,
            Black,
            Brown
        }
        public Queue<GameObject> obj = null;
        public string name = string.Empty;
        public skinType skin = skinType.Yellow;

        public static skinType ChangeSkinType(string type)
        {
            switch (type)
            {
                case "Y":
                    return skinType.Yellow;
                case "B":
                    return skinType.Black;
                case "W":
                    return skinType.White;
                default:
                    return skinType.Brown;
            }
        }
    }
    public enum ModelType
    {
        Head,
        Body,
        Machine,
        Glove,
        Bat,
        HotShotBat,
        Pitcher,
        PlayerBottom,
        GloveMat,
        MachineMat,
    }
    public enum EquipSkinType
    {
        Red,
        Yellow,
        Blue,
    }
    public enum GameType
    {
        InGame,
        OutGame,
    }
    [Header("머리")]
    public Dictionary<int, ModelClass> Heads = new Dictionary<int, ModelClass>();
    [Header("몸")]
    public Dictionary<int, Queue<GameObject>> Bodys = new Dictionary<int, Queue<GameObject>>();
    [Header("투수 장비")]
    public Dictionary<int, Queue<GameObject>> Machines = new Dictionary<int, Queue<GameObject>>();
    [Header("글러브")]
    public Dictionary<int, Queue<GameObject>> Gloves = new Dictionary<int, Queue<GameObject>>();
    [Header("인게임 투수")]
    public Dictionary<int, Queue<GameObject>> Pitchers = new Dictionary<int, Queue<GameObject>>();
    [Header("베트")]
    public Dictionary<int, Material> Bats = new Dictionary<int, Material>();
    [Header("글러브 Material")]
    public Dictionary<int, Material> GloveMats = new Dictionary<int, Material>();
    [Header("머신 Material")]
    public Dictionary<int, Material> MachineMats = new Dictionary<int, Material>();
    [Header("HotShot 베트")]
    public Dictionary<int, Material> HotShotBats = new Dictionary<int, Material>();

    //저장하고 있을 것
    [Header("스킨")]
    public Dictionary<ModelClass.skinType, Material> Skins = new Dictionary<ModelClass.skinType, Material>();
    [Header("발판(원형)")]
    public Dictionary<int, Material> PlayerBottom = new Dictionary<int, Material>();
    [Header("유니폼")]
    public Dictionary<int, Material> Uniforms = new Dictionary<int, Material>();


    string name = string.Empty;
    GameObject newObj = null;
    int count = 0;
    int maxCount = 0;
    GameObject[] arrModel;
    public GameType gameType = GameType.InGame;
    public bool isInit = false;

    #region Addressable
    public override async UniTask LoadAddressable(Action action)
    {
        switch (resourceState)
        {
            case ResourceState.Common:
                {
                    //저장되어있어야 할 리소스(글로벌 매니져만 들고 있음)
                    //피부 
                    IList<IResourceLocation> m_skins = await Addressables.LoadResourceLocationsAsync("Skin").ToUniTask();
                    for (int i = 0; i < m_skins.Count; i++)
                    {
                        string temp = m_skins[i].PrimaryKey;
                        var obj = await Addressables.LoadAssetAsync<Material>(temp).ToUniTask();
                        string[] name = m_skins[i].PrimaryKey.Split('_');
                        if(!Skins.ContainsKey(ModelClass.ChangeSkinType(name[name.Length - 1])))
                            Skins.Add(ModelClass.ChangeSkinType(name[name.Length - 1]), obj);
                        else
                        {
                            Skins.Remove(ModelClass.ChangeSkinType(name[name.Length - 1]));
                            Skins.Add(ModelClass.ChangeSkinType(name[name.Length - 1]), obj);
                        }
                    }

                    //유니폼
                    IList<IResourceLocation> m_uniforms = await Addressables.LoadResourceLocationsAsync("Uniform").ToUniTask();
                    for (int i = 0; i < m_uniforms.Count; i++)
                    {
                        string temp = m_uniforms[i].PrimaryKey;
                        string[] name = temp.Split('_');
                        var obj = await Addressables.LoadAssetAsync<Material>(temp).ToUniTask();
                        AddKey(Uniforms, int.Parse(name[1]), obj);
                    }

                    //원형 발판
                    IList<IResourceLocation> m_playerBottom = await Addressables.LoadResourceLocationsAsync("PlayerBottom").ToUniTask();
                    for (int i = 0; i < m_playerBottom.Count; i++)
                    {
                        string temp = m_playerBottom[i].PrimaryKey;
                        string[] name = temp.Split('_');
                        var obj = await Addressables.LoadAssetAsync<Material>(temp).ToUniTask();
                        AddKey(PlayerBottom, int.Parse(name[name.Length - 1]), obj);
                    }
                }
                break;
            case ResourceState.OutGame:
            case ResourceState.InGame:
                {
                    AllClear();
                    IList<IResourceLocation> m_heads = await Addressables.LoadResourceLocationsAsync("HeadParts").ToUniTask();
                    for (int i = 0; i < m_heads.Count; i++)
                    {
                        ModelClass mc = new ModelClass();
                        mc.obj = new Queue<GameObject>();
                        mc.name = m_heads[i].PrimaryKey;
                        string[] arrName = mc.name.Split('_');
                        mc.skin = ModelClass.ChangeSkinType(arrName[2]);
                        if(!Heads.ContainsKey(int.Parse(arrName[1])))
                            Heads.Add(int.Parse(arrName[1]), mc);
                        else
                        {
                            Heads.Remove(int.Parse(arrName[1]));
                            Heads.Add(int.Parse(arrName[1]), mc);
                        }
                    }

                    IList<IResourceLocation> m_bodys = await Addressables.LoadResourceLocationsAsync("BodyModel").ToUniTask();
                    for (int i = 1; i <= m_bodys.Count; i++)
                        AddKey(Bodys, i, new Queue<GameObject>());

                    IList<IResourceLocation> m_Machines = await Addressables.LoadResourceLocationsAsync("MachineItem").ToUniTask();
                    for (int i = 1; i <= m_Machines.Count; i++)
                        AddKey(Machines, i, new Queue<GameObject>());

                    IList<IResourceLocation> m_gloves = await Addressables.LoadResourceLocationsAsync("Glove").ToUniTask();
                    for (int i = 1; i <= m_gloves.Count; i++)
                        AddKey(Gloves, i, new Queue<GameObject>());

                    IList<IResourceLocation> m_pitchers = await Addressables.LoadResourceLocationsAsync("Machine").ToUniTask();
                    for (int i = 1; i <= m_pitchers.Count; i++)
                        AddKey(Pitchers, i, new Queue<GameObject>());

                    IList<IResourceLocation> m_bats = await Addressables.LoadResourceLocationsAsync("Bat").ToUniTask();
                    for (int i = 1; i <= m_bats.Count; i++)
                        AddKey(Bats, i, null);
                    IList<IResourceLocation> m_gloveMats = await Addressables.LoadResourceLocationsAsync("GloveMat").ToUniTask();
                    for (int i = 1; i <= m_gloveMats.Count; i++)
                        AddKey(GloveMats, i, null);

                    IList<IResourceLocation> m_machineMats = await Addressables.LoadResourceLocationsAsync("MachineMat").ToUniTask();
                    for (int i = 1; i <= m_machineMats.Count; i++)
                        AddKey(MachineMats, i, null);
                    //나중에 찾아 볼것 
                    HotShotBats.Clear();
                    IList<IResourceLocation> m_hotShotBats = await Addressables.LoadResourceLocationsAsync("HotShotBat").ToUniTask();
                    for (int i = 1; i <= m_hotShotBats.Count; i++)
                        AddKey(HotShotBats, i, null);
                }
                break;
        }

        action.Invoke();
    }
    #endregion
    void AddKey(Dictionary<int,Queue<GameObject>> dic,int key,Queue<GameObject> value)
    {
        if (!dic.ContainsKey(key))
            dic.Add(key, value);
        else
        {
            ErrorMassage(dic.ToString(), key);
            dic.Remove(key);
            dic.Add(key, value);
        }
    }

    void AddKey(Dictionary<int, Material> dic, int key, Material value)
    {
        if (!dic.ContainsKey(key))
            dic.Add(key, value);
        else
        {
            ErrorMassage(dic.ToString(), key);
            dic.Remove(key);
            dic.Add(key, value);
        }
    }
    //머리 가져올때 
    public async UniTask<GameObject> GetHead(ModelType type, int num)
    {
        GameObject obj = null;
        Dictionary<int, Queue<GameObject>> models = ReturnType(type);
        if (Heads[num].obj.Count == 0)
        {
            obj = await CreateObj(Heads[num].name, num);
        }
        else
            obj = Heads[num].obj.Dequeue();
        return obj;
    }
    //오브젝트 하나만 가져올때 
    public async UniTask<GameObject> GetModel(ModelType type, int num)
    {
        GameObject obj = null;
        Dictionary<int, Queue<GameObject>> models = ReturnType(type);
        if (models[num].Count == 0)
        {
            obj = await CreateObj(name + num.ToString(), num);
        }
        else
            obj = models[num].Dequeue();
        return obj;
    }
    //머테리얼 가져올때
    public async UniTask<Material> GetModelMat(ModelType type, int num, int imageIdx = 0)
    {
        Dictionary<int, Material> models;
        int idx = num;
        if (type == ModelType.Bat)
            models = Bats;
        else if (type == ModelType.HotShotBat)
            models = HotShotBats;
        else if (type == ModelType.GloveMat)
        {
            models = GloveMats;
            idx = imageIdx;
        }
        else if (type == ModelType.MachineMat)
        {
            models = MachineMats;
            idx = imageIdx;
        }
        else
            models = Bats;
        if (models[idx] == null)
        {
            if (type == ModelType.Bat || type == ModelType.HotShotBat)
                models[idx] = await CreateMat(ReturnTypeName(type) + num.ToString(), num, type, models);
            if(type == ModelType.GloveMat || type == ModelType.MachineMat)
            {
                models[idx] = await CreateMat(ReturnTypeName(type)+num.ToString()+ReturnEquipSkinName(imageIdx), idx, type, models);
            }
        }
        return models[idx];
    }
    //오브젝트 여러개 가져올때
    public async UniTask<GameObject[]> GetModel(ModelType type, int[] num)
    {
        Dictionary<int, Queue<GameObject>> models = ReturnType(type);
        arrModel = new GameObject[num.Length];
        count = 0;
        maxCount = num.Length;
        Queue<string> queName = new Queue<string>();
        Queue<int> queIdx = new Queue<int>();
        if (type == ModelType.Head)
        {
            for (int i = 0; i < num.Length; i++)
            {
                if (Heads[num[i]].obj.Count == 0)
                {
                    queIdx.Enqueue(i);
                    queName.Enqueue(Heads[num[i]].name);
                }
                else
                {
                    arrModel[i] = Heads[num[i]].obj.Dequeue();
                    count++;
                }
            }
            if (count < maxCount)
                arrModel = await CreateObj(queName, queIdx);
        }
        else
        {
            for (int i = 0; i < num.Length; i++)
            {
                if (models[num[i]].Count == 0)
                {
                    if (type == ModelType.Body)
                    {
                        queIdx.Enqueue(i);
                        queName.Enqueue(ReturnBodyName(num[i]));
                    }
                    else
                    {
                        queIdx.Enqueue(i);
                        queName.Enqueue(name + num[i].ToString());
                    }
                }
                else
                {
                    arrModel[i] = models[num[i]].Dequeue();
                    count++;
                }
            }
            if (count < maxCount)
                arrModel = await CreateObj(queName, queIdx);
        }
        return arrModel;
    }
    Dictionary<int, Queue<GameObject>> ReturnType(ModelType type, int bodyType = 0)
    {
        name = ReturnTypeName(type);
        switch (type)
        {
            case ModelType.Body:
                return Bodys;
            case ModelType.Machine:
                return Machines;
            case ModelType.Glove:
                return Gloves;
            case ModelType.Bat:
                return null;
            case ModelType.Pitcher:
                return Pitchers;
            default:
                return null;
        }
    }
    string ReturnTypeName(ModelType type)
    {
        switch (type)
        {
            case ModelType.Machine:
                return "PitMachine_";
            case ModelType.Glove:
                return "Defence_Glove_";
            case ModelType.Bat:
                return "Bat_";
            case ModelType.HotShotBat:
                return "Bat_HotShot_";
            case ModelType.Pitcher:
                return "Pitcher_";
            case ModelType.PlayerBottom:
                return "ChaBottom_LV_";
            case ModelType.GloveMat:
                return "Defence_glove";
            case ModelType.MachineMat:
                return "PitM_";
            default:
                return string.Empty;
        }
    }
    string ReturnBodyName(int num)
    {
        switch (num)
        {
            case 1:
                return "Swing";
            case 2:
                return "Pitching";
            case 3:
                return "Defender";
            case 4:
                return "Runner";
            default:
                return string.Empty;
        }
    }
    string ReturnEquipSkinName(int idx)
    {
        idx = ((idx - 1) % 3) + 1;
        switch (idx)
        {
            case 1:
                return "_B";
            case 2:
                return "_R";
            case 3:
                return "_Y";
            default:
                return "_B";
        }
    }
    async UniTask<GameObject> CreateObj(string name, int num)
    {
        var obj = await Addressables.InstantiateAsync(name).ToUniTask();
        newObj = obj;
        newObj.name = newObj.name.Replace("(Clone)", "");
        return obj;
    }
    async UniTask<GameObject[]> CreateObj(Queue<string> name, Queue<int> idx)
    {
        GameObject obj = null;
        int count = name.Count;
        for (int i = 0; i < count; i++)
        {
            obj = await Addressables.InstantiateAsync(name.Dequeue()).ToUniTask();
            obj.name = obj.name.Replace("(Clone)", "");
            arrModel[idx.Dequeue()] = obj;
        }
        return arrModel;
    }
    async UniTask<Material> CreateMat(string name, int num, ModelType type, Dictionary<int,Material> models)
    {
        var obj = await Addressables.LoadAssetAsync<Material>(name).ToUniTask();

        models[num] = obj;

        return models[num];
    }
    public void ReturnObj(ModelType type, GameObject obj)
    {
        Transform parPos;
        if (gameType == GameType.InGame)
            parPos = IMgr.Inst.modelStorage.transform;
        else
            parPos = OMgr.Inst.modelStorage.transform;
        if (type == ModelType.Head)
        {
            Heads[int.Parse(obj.name.Split('_')[1])].obj.Enqueue(obj);
        }
        else
        {
            Dictionary<int, Queue<GameObject>> models = ReturnType(type);
            string[] name = obj.name.Split('_');
            models[int.Parse(name[name.Length - 1])].Enqueue(obj);
        }
        obj.transform.SetParent(parPos);
        obj.SetActive(false);
    }
    public Material GetSkin(GameObject head)
    {
        if (!head.name.Contains("Head")) return null;
        string[] name = head.name.Split('_');

        return Skins[ModelClass.ChangeSkinType(name[name.Length - 1])];

    }
    public Material GetUniform(int idx, bool isHome = false)
    {
        if (isHome) return Uniforms[0];
        return Uniforms[idx + 1];
    }
    public Material GetPlayerBottom(int idx)
    {
        if (PlayerBottom.ContainsKey(idx))
            return PlayerBottom[idx];
        return PlayerBottom[1];
    }
    public void AllClear()
    {
        Heads.Clear();
        Bodys.Clear();
        Gloves.Clear();
        Machines.Clear();
        Bats.Clear();
        Pitchers.Clear();
        GloveMats.Clear();
        MachineMats.Clear();
    }
    void ErrorMassage(string str , int key)
    {
        CustomLog.Error(WLogType.all, str, " : ", key.ToString());
    }
}
