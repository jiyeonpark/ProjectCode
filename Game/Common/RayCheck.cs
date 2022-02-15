using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCheck : MonoBehaviour
{
    static private int count = 0;
    static private RaycastHit PickScreenInfo;   // 중복 raycast를 방지하기위함 : 해당 업데이트에 raycast를 호출했으면 담겨있을것임...
    private void LateUpdate()
    {

        //if (count > 0) CustomLog.Log(WLogType.debug, " +++ ", count.ToString());
        count = 0;
        PickScreenInfo.distance = 0;
    }

    static public bool PickScreenRay(out RaycastHit hitInfo)
    {
        if (PickScreenInfo.distance != 0)
        {// 이미 raycast를 호출한상태 = 값이 들어있다..
            hitInfo = PickScreenInfo;
            return true;
        }

        if (CustomInput.IsPointerOverGameObject())
        {// UI 위에 있다..
            hitInfo = PickScreenInfo;
            return false;  
        }

        Vector3 screenvec = CustomInput.GetInputPosition();
        Ray ray = IMgr.Inst._CameraMgr.Cam.ScreenPointToRay(screenvec);

        count++;
        if (Physics.Raycast(ray, out hitInfo))
        {
            PickScreenInfo = hitInfo;
            return true;
        }
        return false;
    }

    static public bool PickScreenRay(float dis, out RaycastHit hitInfo)
    {
        if (PickScreenInfo.distance != 0)
        {// 이미 raycast를 호출한상태 = 값이 들어있다..
            hitInfo = PickScreenInfo;
            return true;
        }

        if (CustomInput.IsPointerOverGameObject())
        {// UI 위에 있다..
            hitInfo = PickScreenInfo;
            return false;
        }

        Vector3 screenvec = CustomInput.GetInputPosition();
        Ray ray = IMgr.Inst._CameraMgr.Cam.ScreenPointToRay(screenvec);

        count++;
        if (Physics.Raycast(ray, out hitInfo, dis))
        {
            PickScreenInfo = hitInfo;
            return true;
        }
        return false;
    }

    static public bool PickScreenRay(float dis, string layerName, out RaycastHit hitInfo)
    {
        if (PickScreenInfo.distance != 0)
        {// 이미 raycast를 호출한상태 = 값이 들어있다..
            hitInfo = PickScreenInfo;
            return true;
        }

        if (CustomInput.IsPointerOverGameObject())
        {// UI 위에 있다..
            hitInfo = PickScreenInfo;
            return false;
        }

        Vector3 screenvec = CustomInput.GetInputPosition();
        Ray ray = IMgr.Inst._CameraMgr.Cam.ScreenPointToRay(screenvec);

        count++;
        if (Physics.Raycast(ray, out hitInfo, dis, 1 << LayerMask.NameToLayer(layerName)))
        {
            PickScreenInfo = hitInfo;
            return true;
        }
        return false;
    }

    static public bool DirectRay(Vector3 pos, Vector3 dir, float dis)
    {
        count++;
        if (Physics.Raycast(pos, dir, dis)) return true;
        return false;
    }
}
