using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleManager<T> : MonoBehaviour where T : MonoBehaviour 
{
    protected bool dontDestroy = false;
    private bool firstLoaded = false;

    private static T instance = null;
    public static T Inst
    {
        get
        {
            if (instance == null)
            {
                Debug.LogWarning(typeof(T).ToString() + "'s instance is null.");
            }

            return instance;
        }
    }

    public static bool IsCreate
    {
        get
        {
            if (instance == null)
            {
                return false;
            }
            else
                return true;
        }
    }

    private void Awake()
    {
        if (instance != null)
        {
            if (instance != this as T)
            {
                Destroy(gameObject);
            }

            return;
        }

        if (firstLoaded)
            return;

        if (instance == null)
        {
            instance = this as T;
        }

        OnAwake();        
    }

    protected virtual void OnAwake()
    {
        if (dontDestroy)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        if (firstLoaded)
            return;

        OnStart();

        firstLoaded = true;
    }

    protected virtual void OnStart()
    {
    }

    void OnDestory()
    {
        Destory();
    }

    protected virtual void Destory()
    {
        instance = null;
        firstLoaded = false;
    }

    protected virtual void ReleaseData()
    {

    }

}
