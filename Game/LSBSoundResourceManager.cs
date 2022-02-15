using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using LetsBaseball.MasterData;
using System;
using UnityEngine.Audio;

[System.Serializable]
public class AudioPool
{
    public AudioSource source = null;

    private List<AudioSource> pool = new List<AudioSource>();
    public AudioSource GetAudio()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i].isPlaying == false) return pool[i];
        }

        AudioSource createaudio = GameObject.Instantiate(source, source.transform.parent);
        pool.Add(createaudio);

        createaudio.gameObject.AddComponent<SoundControl>();
        return createaudio;
    }

    public AudioSource GetPlayAudio(List<AudioClip> clips)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if(pool[i].isPlaying && clips.Contains(pool[i].clip))
            {
                return pool[i];
            }
        }
        return null;
    }
}

public class LSBSoundResourceManager : LoadBehaviour
{
    //public static LSBSoundResourceManager Manager = new LSBSoundResourceManager();

    SoundManagerSO CommonSound = null;
    SoundManagerSO OutgameSound = null;
    SoundManagerSO IngameSound = null;

    ResourceSoundManager soundManager = null;

    public AudioPool bgmPlayer = null;
    public AudioPool uiPlayer = null;
    public AudioPool effectPlayer = null;

    bool isLoadManagers = false;

    public bool IsLoadManagers() { return isLoadManagers; }

    public override async UniTask LoadAddressable(Action action)
    {
        var soundmanager = await Addressables.LoadAssetAsync<ResourceSoundManager>("ResourceSoundManager_SO").ToUniTask();
        if (soundmanager != null)
        {
            soundManager = soundmanager;
            CustomLog.Log(WLogType.debug, "========== LoadResourceSoundManager : ", soundManager);
        }

        switch (resourceState)
        {
            case ResourceState.Common:
                CommonSound = await Addressables.LoadAssetAsync<SoundManagerSO>("SoundCommon_SO").ToUniTask();
                break;
            case ResourceState.OutGame:
                OutgameSound = await Addressables.LoadAssetAsync<SoundManagerSO>("SoundOutgame_SO").ToUniTask();
                break;
            case ResourceState.InGame:
                IngameSound = await Addressables.LoadAssetAsync<SoundManagerSO>("SoundIngame_SO").ToUniTask();
                break;
        }

        var audioMixerGroups = soundManager.AudioMixerObject.FindMatchingGroups(string.Empty);

        foreach (AudioMixerGroup AMG in audioMixerGroups)
        {
            if (AMG.name == "BGM")
                bgmPlayer.source.outputAudioMixerGroup = AMG;
            else if(AMG.name == "UI")
                uiPlayer.source.outputAudioMixerGroup = AMG;
            else if(AMG.name == "Effect")
                effectPlayer.source.outputAudioMixerGroup = AMG;
        }

        action.Invoke();
    }

    #region Fun

    /// <summary>
    /// 기존 enum 사용 재생
    /// </summary>
    /// <param name="clip">새로운 enum</param>
    public void PlaySound(SoundClip clip, float volume = -1,float soundLength = 0)
    {
        if(clip > SoundClip.CommonUiNone && clip < SoundClip.CommonUiMax)
        {
            //Common ui sound
            PlayCommonSound(SoundResourceGroup.Ui, (int)clip - (int)SoundClip.CommonUiNone,volume,soundLength);
        }
        else if(clip > SoundClip.CommonEffectNone && clip < SoundClip.CommonEffectMax)
        {
            //Common effect sound
            PlayCommonSound(SoundResourceGroup.Effect, (int)clip - (int)SoundClip.CommonEffectNone);
        }
        else if(clip > SoundClip.OutUiNone && clip < SoundClip.OutUiMax)
        {
            // out ui sound
            PlayOutgameSound(SoundResourceGroup.Ui, (int)clip - (int)SoundClip.OutUiNone);
        }
        else if (clip > SoundClip.OutBgmNone && clip < SoundClip.OutBgmMax)
        {
            // out bgm sound
            PlayOutgameSound(SoundResourceGroup.Bgm, (int)clip - (int)SoundClip.OutBgmNone);
        }
        else if (clip > SoundClip.OutEffectNone && clip < SoundClip.OutEffectMax)
        {
            // out bgm sound
            PlayOutgameSound(SoundResourceGroup.Effect, (int)clip - (int)SoundClip.OutEffectNone);
        }
        else if (clip > SoundClip.InBgmNone && clip < SoundClip.InBgmMax)
        {
            PlayIngameSound(SoundResourceGroup.Bgm, (int)clip - (int)SoundClip.InBgmNone);
        }
        else if (clip > SoundClip.InEffectNone && clip < SoundClip.InEffectMax)
        {
            // in effect sound
            PlayIngameSound(SoundResourceGroup.Effect, (int)clip - (int)SoundClip.InEffectNone);
        }
        else
            CustomLog.Error(WLogType.all, "SoundClip Out Of Range.");
    }

    public void StopSound(SoundClip clip)
    {
        if (clip > SoundClip.CommonUiNone && clip < SoundClip.CommonUiMax)
        {
            //Common ui sound
            StopCommonSound(SoundResourceGroup.Ui, (int)clip - (int)SoundClip.CommonUiNone);
        }
        else if (clip > SoundClip.CommonEffectNone && clip < SoundClip.CommonEffectMax)
        {
            //Common effect sound
            StopCommonSound(SoundResourceGroup.Effect, (int)clip - (int)SoundClip.CommonEffectNone);
        }
        else if (clip > SoundClip.OutUiNone && clip < SoundClip.OutUiMax)
        {
            // out ui sound
            StopOutgameSound(SoundResourceGroup.Ui, (int)clip - (int)SoundClip.OutUiNone);
        }
        else if (clip > SoundClip.OutBgmNone && clip < SoundClip.OutBgmMax)
        {
            // out bgm sound
            StopOutgameSound(SoundResourceGroup.Bgm, (int)clip - (int)SoundClip.OutBgmNone);
        }
        else if (clip > SoundClip.OutEffectNone && clip < SoundClip.OutEffectMax)
        {
            // out effect sound
            StopOutgameSound(SoundResourceGroup.Effect, (int)clip - (int)SoundClip.OutEffectNone);
        }
        else if (clip > SoundClip.InBgmNone && clip < SoundClip.InBgmMax)
        {
            StopIngameSound(SoundResourceGroup.Bgm, (int)clip - (int)SoundClip.InBgmNone);
        }
        else if (clip > SoundClip.InEffectNone && clip < SoundClip.InEffectMax)
        {
            // in effect sound
            StopIngameSound(SoundResourceGroup.Effect, (int)clip - (int)SoundClip.InEffectNone);
        }
        else
            CustomLog.Error(WLogType.all, "SoundClip Out Of Range.");
    }

    /// <summary>
    /// 공통사운드 재생
    /// </summary>
    /// <param name="soundGroup">사운드 종류. ex) Bgm, Effect, UI</param>
    /// <param name="index">사운드의 TypeIndex</param>
    public void PlayCommonSound(SoundResourceGroup soundGroup, int index, float volume = -1, float soundLength = 0)
    {
        PlaySound(SoundResourceTypeList.Common, soundGroup, index, volume, soundLength);
    }

    /// <summary>
    /// 공통사운드 정지
    /// </summary>
    /// <param name="soundGroup">사운드 종류. ex) Bgm, Effect, UI</param>
    /// <param name="index">사운드의 TypeIndex</param>
    public void StopCommonSound(SoundResourceGroup soundGroup, int index)
    {
        StopSound(SoundResourceTypeList.Common, soundGroup, index);
    }

    /// <summary>
    /// 인게임사운드 재생
    /// </summary>
    /// <param name="soundGroup">사운드 종류. ex) Bgm, Effect, UI</param>
    /// <param name="index">사운드의 TypeIndex</param>
    public void PlayIngameSound(SoundResourceGroup soundGroup, int index, float volume = -1, float soundLength = 0)
    {
        PlaySound(SoundResourceTypeList.Ingame, soundGroup, index, volume, soundLength);
    }

    /// <summary>
    /// 인게임사운드 정지
    /// </summary>
    /// <param name="soundGroup">사운드 종류. ex) Bgm, Effect, UI</param>
    /// <param name="index">사운드의 TypeIndex</param>
    public void StopIngameSound(SoundResourceGroup soundGroup, int index)
    {
        StopSound(SoundResourceTypeList.Ingame, soundGroup, index);
    }

    /// <summary>
    /// 아웃게임사운드 재생
    /// </summary>
    /// <param name="soundGroup">사운드 종류. ex) Bgm, Effect, UI</param>
    /// <param name="index">사운드의 TypeIndex</param>
    public void PlayOutgameSound(SoundResourceGroup soundGroup, int index, float volume = -1, float soundLength = 0)
    {
        PlaySound(SoundResourceTypeList.Outgame, soundGroup, index, volume, soundLength);
    }

    /// <summary>
    /// 아웃게임사운드 정지
    /// </summary>
    /// <param name="soundGroup">사운드 종류. ex) Bgm, Effect, UI</param>
    /// <param name="index">사운드의 TypeIndex</param>
    public void StopOutgameSound(SoundResourceGroup soundGroup, int index)
    {
        StopSound(SoundResourceTypeList.Outgame, soundGroup, index);
    }

    void PlaySound(SoundResourceTypeList type, SoundResourceGroup soundGroup, int index, float volume = -1, float soundLength = 0)
    {
        SoundManagerSO m_soundmanager = null;

        switch(type)
        {
            case SoundResourceTypeList.Common: m_soundmanager = CommonSound; break;
            case SoundResourceTypeList.Outgame: m_soundmanager = OutgameSound; break;
            case SoundResourceTypeList.Ingame: m_soundmanager = IngameSound; break;
        }

        var sound = m_soundmanager.GetAudio(soundGroup, index);
        AudioPool audioPool = GetAudioPlayer(soundGroup);
        AudioSource audioPlayer = audioPool.GetPlayAudio(sound.source);

        if (audioPlayer == null)
            audioPlayer = audioPool.GetAudio();

        SoundControl s_control = audioPlayer.gameObject.GetComponent<SoundControl>();

        audioPlayer.clip = sound.source[UnityEngine.Random.Range(0, sound.source.Count)];

        if(volume == -1)
            audioPlayer.volume = sound.SoundVolume;
        else
            audioPlayer.volume = volume;

        audioPlayer.loop = sound.isLoop;
        s_control.soundVolume = audioPlayer.volume;

        audioPlayer.time = sound.SoundSkipTime;

        if(sound.SoundPlayTime > 0)
            s_control.SetPlayTime(() => StopSound(type, soundGroup, index), sound.SoundPlayTime);

        if (soundLength != 0)
        {
            CustomLog.Log(WLogType.debug, audioPlayer.time.ToString());
            audioPlayer.time = soundLength;
        }

        audioPlayer.Play();
    }

    void StopSound(SoundResourceTypeList type, SoundResourceGroup soundGroup, int index)
    {
        SoundManagerSO m_soundmanager = null;

        switch (type)
        {
            case SoundResourceTypeList.Common: m_soundmanager = CommonSound; break;
            case SoundResourceTypeList.Outgame: m_soundmanager = OutgameSound; break;
            case SoundResourceTypeList.Ingame: m_soundmanager = IngameSound; break;
        }

        var sound = m_soundmanager.GetAudio(soundGroup, index);
        var audioPool = GetAudioPlayer(soundGroup);
        AudioSource audioPlayer = audioPool.GetPlayAudio(sound.source);

        if (audioPlayer == null)
            return;

        audioPlayer.Stop();
    }

    AudioPool GetAudioPlayer(SoundResourceGroup soundGroup)
    {
        switch (soundGroup)
        {
            case SoundResourceGroup.Bgm:
                return bgmPlayer;
            case SoundResourceGroup.Effect:
                return effectPlayer;
            case SoundResourceGroup.Ui:
                return uiPlayer;
        }

        return null;
    }
    #endregion

    #region Fadein&Fadeout
    /// <summary>
    /// 사운드 페이드아웃(완전 Stop X)
    /// </summary>
    /// <param name="clip">사운드 클립 enum. ex) Bgm, Effect, UI</param>
    /// <param name="fadeoutTime">페이드아웃 되는 시간</param>
    /// <param name="fadeoutVolume">페이드아웃 되는 볼륨(기존 볼륨에 fadeoutVolume을 곱해서 계산)</param>
    public void PadeOutSound(SoundClip clip, float fadeoutTime, float fadeoutVolume)
    {
        if (clip > SoundClip.CommonUiNone && clip < SoundClip.CommonUiMax)
        {
            //Common ui sound
            PadeOutCommonSound(SoundResourceGroup.Ui, (int)clip - (int)SoundClip.CommonUiNone, fadeoutTime, fadeoutVolume);
        }
        else if (clip > SoundClip.CommonEffectNone && clip < SoundClip.CommonEffectMax)
        {
            //Common effect sound
            PadeOutCommonSound(SoundResourceGroup.Effect, (int)clip - (int)SoundClip.CommonEffectNone, fadeoutTime, fadeoutVolume);
        }
        else if (clip > SoundClip.OutUiNone && clip < SoundClip.OutUiMax)
        {
            // out ui sound
            PadeOutOutgameSound(SoundResourceGroup.Ui, (int)clip - (int)SoundClip.OutUiNone, fadeoutTime, fadeoutVolume);
        }
        else if (clip > SoundClip.OutBgmNone && clip < SoundClip.OutBgmMax)
        {
            // out bgm sound
            PadeOutOutgameSound(SoundResourceGroup.Bgm, (int)clip - (int)SoundClip.OutBgmNone, fadeoutTime, fadeoutVolume);
        }
        else if (clip > SoundClip.OutEffectNone && clip < SoundClip.OutEffectMax)
        {
            // out effect sound
            PadeOutOutgameSound(SoundResourceGroup.Effect, (int)clip - (int)SoundClip.OutEffectNone, fadeoutTime, fadeoutVolume);
        }
        else if(clip>SoundClip.InBgmNone && clip < SoundClip.InBgmMax)
        {
            PadeOutIngameSound(SoundResourceGroup.Bgm, (int)clip - (int)SoundClip.InBgmNone, fadeoutTime, fadeoutVolume);
        }
        else if (clip > SoundClip.InEffectNone && clip < SoundClip.InEffectMax)
        {
            // in effect sound
            PadeOutIngameSound(SoundResourceGroup.Effect, (int)clip - (int)SoundClip.InEffectNone, fadeoutTime, fadeoutVolume);
        }
        else
            CustomLog.Error(WLogType.all, "SoundClip Out Of Range.");
    }

    public void PadeOutCommonSound(SoundResourceGroup type, int index, float fadeoutTime, float fadeoutVolume)
    {
        PadeOutSound(SoundResourceTypeList.Common, type, index, fadeoutTime, fadeoutVolume);
    }

    public void PadeOutOutgameSound(SoundResourceGroup type, int index, float fadeoutTime, float fadeoutVolume)
    {
        PadeOutSound(SoundResourceTypeList.Outgame, type, index, fadeoutTime, fadeoutVolume);
    }

    public void PadeOutIngameSound(SoundResourceGroup type, int index, float fadeoutTime, float fadeoutVolume)
    {
        PadeOutSound(SoundResourceTypeList.Ingame, type, index, fadeoutTime, fadeoutVolume);
    }

    void PadeOutSound(SoundResourceTypeList type, SoundResourceGroup soundGroup, int index, float fadeoutTime, float fadeoutVolume)
    {
        SoundManagerSO m_soundmanager = null;

        switch (type)
        {
            case SoundResourceTypeList.Common: m_soundmanager = CommonSound; break;
            case SoundResourceTypeList.Outgame: m_soundmanager = OutgameSound; break;
            case SoundResourceTypeList.Ingame: m_soundmanager = IngameSound; break;
        }

        var sound = m_soundmanager.GetAudio(soundGroup, index);
        var audioPool = GetAudioPlayer(soundGroup);
        AudioSource audioPlayer = audioPool.GetPlayAudio(sound.source);

        if (audioPlayer == null)
            return;

        var s_control = audioPlayer.gameObject.GetComponent<SoundControl>();
        s_control.SetSoundPadeOut(fadeoutTime, fadeoutVolume);
    }

    //
    // 페이드인
    //

    public void PadeInSound(SoundClip clip, float fadeoutTime)
    {
        if (clip > SoundClip.CommonUiNone && clip < SoundClip.CommonUiMax)
        {
            //Common ui sound
            PadeInCommonSound(SoundResourceGroup.Ui, (int)clip - (int)SoundClip.CommonUiNone, fadeoutTime);
        }
        else if (clip > SoundClip.CommonEffectNone && clip < SoundClip.CommonEffectMax)
        {
            //Common effect sound
            PadeInCommonSound(SoundResourceGroup.Effect, (int)clip - (int)SoundClip.CommonEffectNone, fadeoutTime);
        }
        else if (clip > SoundClip.OutUiNone && clip < SoundClip.OutUiMax)
        {
            // out ui sound
            PadeInOutgameSound(SoundResourceGroup.Ui, (int)clip - (int)SoundClip.OutUiNone, fadeoutTime);
        }
        else if (clip > SoundClip.OutBgmNone && clip < SoundClip.OutBgmMax)
        {
            // out bgm sound
            PadeInOutgameSound(SoundResourceGroup.Bgm, (int)clip - (int)SoundClip.OutBgmNone, fadeoutTime);
        }
        else if (clip > SoundClip.OutEffectNone && clip < SoundClip.OutEffectMax)
        {
            // out effect sound
            PadeInOutgameSound(SoundResourceGroup.Effect, (int)clip - (int)SoundClip.OutEffectNone, fadeoutTime);
        }
        else if (clip > SoundClip.InBgmNone && clip < SoundClip.InBgmMax)
        {
            PadeInIngameSound(SoundResourceGroup.Bgm, (int)clip - (int)SoundClip.InBgmNone, fadeoutTime);
        }
        else if (clip > SoundClip.InEffectNone && clip < SoundClip.InEffectMax)
        {
            // in effect sound
            PadeInIngameSound(SoundResourceGroup.Effect, (int)clip - (int)SoundClip.InEffectNone, fadeoutTime);
        }
        else
            CustomLog.Error(WLogType.all, "SoundClip Out Of Range.");
    }

    public void PadeInCommonSound(SoundResourceGroup type, int index, float fadeoutTime)
    {
        PadeInSound(SoundResourceTypeList.Common, type, index, fadeoutTime);
    }

    public void PadeInOutgameSound(SoundResourceGroup type, int index, float fadeoutTime)
    {
        PadeInSound(SoundResourceTypeList.Outgame, type, index, fadeoutTime);
    }

    public void PadeInIngameSound(SoundResourceGroup type, int index, float fadeoutTime)
    {
        PadeInSound(SoundResourceTypeList.Ingame, type, index, fadeoutTime);
    }

    void PadeInSound(SoundResourceTypeList type, SoundResourceGroup soundGroup, int index, float fadeoutTime)
    {
        SoundManagerSO m_soundmanager = null;

        switch (type)
        {
            case SoundResourceTypeList.Common: m_soundmanager = CommonSound; break;
            case SoundResourceTypeList.Outgame: m_soundmanager = OutgameSound; break;
            case SoundResourceTypeList.Ingame: m_soundmanager = IngameSound; break;
        }

        var sound = m_soundmanager.GetAudio(soundGroup, index);
        var audioPool = GetAudioPlayer(soundGroup);
        AudioSource audioPlayer = audioPool.GetPlayAudio(sound.source);

        if (audioPlayer == null)
            return;

        var s_control = audioPlayer.gameObject.GetComponent<SoundControl>();
        s_control.SetSoundPadeIn(fadeoutTime);
    }

    #endregion

    #region AudioMixerControl

    public void InitAudioMixer()
    {
        float BGMValue = GameManager.Inst.optionInfo.isBGMOn ? 1 : 0;
        float SFXValue = GameManager.Inst.optionInfo.isSFXOn ? 1 : 0;

        ChangeAudioMixer(SoundResourceGroup.Bgm, BGMValue);
        ChangeAudioMixer(SoundResourceGroup.Effect, SFXValue);
        ChangeAudioMixer(SoundResourceGroup.Ui, SFXValue);

        ChangeMasterAudio(1);
    }

    public void ChangeAudioMixer(SoundResourceGroup group, float value)
    {
        soundManager.ChangeAudioMixer(group, value);
    }

    public void ChangeMasterAudio(float value)
    {
        soundManager.ChangeMasterAudio(value);
    }

    #endregion
}
