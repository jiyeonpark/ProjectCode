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
                //���ö����� ���̺�
                LSBLocalize.Manager.LoadLocalizeData();
                //�̹��� ��Ʈ �ε�.
                //�̹��� ��Ʈ�� �켱 common ���� �ε�.
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
