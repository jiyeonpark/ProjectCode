using System;
using Cysharp.Threading.Tasks;
using LetsBaseball.MasterData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using FrameWork.UI;

public class UIResourceLoadManager : LoadBehaviour
{
    public override async UniTask LoadAddressable(Action action)
    {
        await LSBUIResourceManager.Manager.LoadResourceManagers();
        ImageResourceTypeList[] arrayType = null;
        //아웃게임에서 로드 하는지 인게임에서 로드 하는지 상황에 따른 아틀라스 로드를 결정.
        //아틀라스 릴리즈 관리도 필요.
        switch (resourceState)
        {
            case ResourceState.Common:
                arrayType =  LoadCommon();
                break;
            case ResourceState.OutGame:
                arrayType = LoadOutGame();
                break;
            case ResourceState.InGame:
                arrayType = LoadInGame();
                break;
        }
        await LSBUIResourceManager.Manager.AsyncPreLoadAtlas(arrayType);
        action.Invoke();

        CustomLog.Log(WLogType.debug, "========== LoadAddressable UI Resource");
    }

    ImageResourceTypeList[] LoadCommon()
    {
        ImageResourceTypeList[] arrayType = new ImageResourceTypeList[]{
            ImageResourceTypeList.Bg,
            ImageResourceTypeList.Cover,
            ImageResourceTypeList.Gage,
            ImageResourceTypeList.Playercard,
            ImageResourceTypeList.Stat,
            ImageResourceTypeList.Skill,
            ImageResourceTypeList.Playerattribute,
            ImageResourceTypeList.Pack,
            ImageResourceTypeList.Iconcard,
            ImageResourceTypeList.Newshop,
            ImageResourceTypeList.Shopiconcat,
            ImageResourceTypeList.Shopiconruby,
            ImageResourceTypeList.Shotslot,
            ImageResourceTypeList.Itemmachine,
            ImageResourceTypeList.Itemglove,
            ImageResourceTypeList.Itembat,
            ImageResourceTypeList.Userprofilepicturegrade,
            ImageResourceTypeList.Nationflagbig,
            ImageResourceTypeList.Nationflagsmall,
            ImageResourceTypeList.Capsule,
            ImageResourceTypeList.Cardbtnbg,
            ImageResourceTypeList.Popupbtnbg,
            ImageResourceTypeList.Rankingslotbg,
            ImageResourceTypeList.Rankingnumber,
            ImageResourceTypeList.Leagueimagebig,
            ImageResourceTypeList.Leagueimagesmall,
            ImageResourceTypeList.Leagueimagetext,
            ImageResourceTypeList.Lobbyiconruby,
            ImageResourceTypeList.Emblemmark,
            ImageResourceTypeList.Emblembg,
            ImageResourceTypeList.Mailicon,
            ImageResourceTypeList.Stadium,
            ImageResourceTypeList.Settingbtnimg,
        };

        return arrayType;
    }

    ImageResourceTypeList[] LoadOutGame()
    {
        ImageResourceTypeList[] arrayType = new ImageResourceTypeList[]{
            ImageResourceTypeList.Bg,
            ImageResourceTypeList.Cover,
            ImageResourceTypeList.Gage,
            ImageResourceTypeList.Playercard,
            ImageResourceTypeList.Stat,
            ImageResourceTypeList.Skill,
            ImageResourceTypeList.Playerattribute,
            ImageResourceTypeList.Pack,
            ImageResourceTypeList.Iconcard,
            ImageResourceTypeList.Newshop,
            ImageResourceTypeList.Shopiconcat,
            ImageResourceTypeList.Shopiconruby,
            ImageResourceTypeList.Shotslot,
            ImageResourceTypeList.Itemmachine,
            ImageResourceTypeList.Itemglove,
            ImageResourceTypeList.Itembat,
            ImageResourceTypeList.Userprofilepicturegrade,
            ImageResourceTypeList.Nationflagbig,
            ImageResourceTypeList.Nationflagsmall,
            ImageResourceTypeList.Capsule,
            ImageResourceTypeList.Cardbtnbg,
            ImageResourceTypeList.Popupbtnbg,
            ImageResourceTypeList.Rankingslotbg,
            ImageResourceTypeList.Rankingnumber,
            ImageResourceTypeList.Leagueimagebig,
            ImageResourceTypeList.Leagueimagesmall,
            ImageResourceTypeList.Leagueimagetext,
            ImageResourceTypeList.Lobbyiconruby,
            ImageResourceTypeList.Emblemmark,
            ImageResourceTypeList.Emblembg,
            ImageResourceTypeList.Mailicon,
            ImageResourceTypeList.Stadium,
            ImageResourceTypeList.Settingbtnimg,
        };

        return arrayType;
    }
    ImageResourceTypeList[] LoadInGame()
    {
        ImageResourceTypeList[] arrayType = new ImageResourceTypeList[]{
            ImageResourceTypeList.Bg,
            ImageResourceTypeList.Cover,
            ImageResourceTypeList.Gage,
            ImageResourceTypeList.Playercard,
            ImageResourceTypeList.Stat,
            ImageResourceTypeList.Skill,
            ImageResourceTypeList.Playerattribute,
            ImageResourceTypeList.Pack,
            ImageResourceTypeList.Iconcard,
            ImageResourceTypeList.Newshop,
            ImageResourceTypeList.Shopiconcat,
            ImageResourceTypeList.Shopiconruby,
            ImageResourceTypeList.Shotslot,
            ImageResourceTypeList.Itemmachine,
            ImageResourceTypeList.Itemglove,
            ImageResourceTypeList.Itembat,
            ImageResourceTypeList.Userprofilepicturegrade,
            ImageResourceTypeList.Nationflagbig,
            ImageResourceTypeList.Nationflagsmall,
            ImageResourceTypeList.Capsule,
            ImageResourceTypeList.Cardbtnbg,
            ImageResourceTypeList.Popupbtnbg,
            ImageResourceTypeList.Rankingslotbg,
            ImageResourceTypeList.Rankingnumber,
            ImageResourceTypeList.Leagueimagebig,
            ImageResourceTypeList.Leagueimagesmall,
            ImageResourceTypeList.Leagueimagetext,
            ImageResourceTypeList.Lobbyiconruby,
            ImageResourceTypeList.Emblemmark,
            ImageResourceTypeList.Emblembg,
            ImageResourceTypeList.Mailicon,
            ImageResourceTypeList.Settingbtnimg,
        };

        return arrayType;
    }

}
