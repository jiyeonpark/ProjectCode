using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class CustomInput
{
    static public bool IsMobile()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            return true;
        return false;
    }

    static public bool CheckInputDown(int count)
    {
        if (IsMobile())
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (EventSystem.current.IsPointerOverGameObject(i) == true)
                    return false;
            }

            if (Input.touchCount > count && Input.GetTouch(count).phase == TouchPhase.Began)
                return true;
        }
        else
        {
            if (EventSystem.current.IsPointerOverGameObject() == true)
                return false;

            if (Input.GetMouseButtonDown(count))
                return true;
        }
        return false;
    }

    static public bool CheckInput(int count)
    {
        if (IsMobile())
        {
            if (Input.touchCount > count && (Input.GetTouch(count).phase == TouchPhase.Moved || Input.GetTouch(count).phase == TouchPhase.Stationary))
                return true;
        }
        else
        {
            if (Input.GetMouseButton(count))
                return true;
        }
        return false;
    }

    static public bool CheckInputUp(int count)
    {
        if (IsMobile())
        {
            if (Input.touchCount > count && Input.GetTouch(count).phase == TouchPhase.Ended)
                return true;
        }
        else
        {
            if (Input.GetMouseButtonUp(count))
                return true;
        }
        return false;
    }

    static public Vector3 GetInputPosition(int touchID = 0)
    {
        if (IsMobile())
        {
            if (Input.touchCount > touchID)
                return Define.WVector3(Input.GetTouch(touchID).position.x, Input.GetTouch(touchID).position.y, 0f);
        }
        else
        {
            return Input.mousePosition;
        }
        return Vector3.zero;
    }

    static public Vector3 prevDelta = Vector3.zero;
    static public Vector3 GetInputDelta()
    {
        Vector3 delta = Vector3.zero;
        if (CheckInputDown(0)) prevDelta = GetInputPosition();
        if (CheckInput(0))
        {
            delta = prevDelta - GetInputPosition();
            prevDelta = GetInputPosition();
        }
        return delta;
    }

    static public bool IsPointerOverGameObject()
    {
        if (IsMobile())
        {
            if (Input.touchCount > 0)
                return EventSystem.current.IsPointerOverGameObject(0);
        }
        else
        {
            return EventSystem.current.IsPointerOverGameObject();
        }
        return false;
    }
}
