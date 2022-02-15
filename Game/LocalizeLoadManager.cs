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
public class LocalizeLoadManager : LoadBehaviour
{
    public override async UniTask LoadAddressable(Action action)
    {

        switch (resourceState)
        {
            case ResourceState.Common:
                //로컬라이즈 테이블
                LSBLocalize.Manager.LoadLocalizeData();
                //이미지 폰트 로딩.
                //이미지 폰트도 우선 common 에서 로딩.
                await LSBLocalize.Manager.AsyncPreLoadAtlasAll();

                break;
            case ResourceState.OutGame:
                break;
            case ResourceState.InGame:
                break;
        }


        action.Invoke();
        CustomLog.Log(WLogType.debug, "========== LoadAddressable Localize Resource");
    }

}
