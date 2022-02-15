using System;
using System.Text;
using UnityEngine;

public enum WLogType
{
    none = 0,           // log 없음
    all,                // log 모두 남김
    debug,              // 디버그일경우에만 log 남김
    release,            // 필수 log 남김

    // 개인 test code..
    jiyeon = 100,
    raejun,
    donghee,
    gwiung,
    jibyeong,
}

public static class CustomLog
{
    public static WLogType logtype = WLogType.all;
    static private StringBuilder stringBuilder = new StringBuilder();

    static public void Log(WLogType type, params string[] list)
    {
        if (logtype != WLogType.all)
        {
            if (logtype == WLogType.none) return;
            else if (logtype != type) return;
        }

        stringBuilder.Length = 0;
        for (int i = 0; i < list.Length; i++)
            stringBuilder.Append(list[i]);
        Debug.Log(stringBuilder.ToString());
    }

    static public void Log(WLogType type, params object[] list)
    {
        if (logtype != WLogType.all)
        {
            if (logtype == WLogType.none) return;
            else if (logtype != type) return;
        }

        stringBuilder.Length = 0;
        for (int i = 0; i < list.Length; i++)
            stringBuilder.Append(list[i]);
        Debug.Log(stringBuilder.ToString());
    }

    static public void Warning(WLogType type, params string[] list)
    {
        if (logtype != WLogType.all)
        {
            if (logtype == WLogType.none) return;
            else if (logtype != type) return;
        }

        stringBuilder.Length = 0;
        for (int i = 0; i < list.Length; i++)
            stringBuilder.Append(list[i]);
        Debug.LogWarning(stringBuilder.ToString());
    }

    static public void Warning(WLogType type, params object[] list)
    {
        if (logtype != WLogType.all)
        {
            if (logtype == WLogType.none) return;
            else if (logtype != type) return;
        }

        stringBuilder.Length = 0;
        for (int i = 0; i < list.Length; i++)
            stringBuilder.Append(list[i]);
        Debug.LogWarning(stringBuilder.ToString());
    }

    static public void Error(WLogType type, params string[] list)
    {
        if (logtype != WLogType.all)
        {
            if (logtype == WLogType.none) return;
            else if (logtype != type) return;
        }

        stringBuilder.Length = 0;
        for (int i = 0; i < list.Length; i++)
            stringBuilder.Append(list[i]);
        Debug.LogError(stringBuilder.ToString());
    }

    static public void Error(WLogType type, params object[] list)
    {
        if (logtype != WLogType.all)
        {
            if (logtype == WLogType.none) return;
            else if (logtype != type) return;
        }

        stringBuilder.Length = 0;
        for (int i = 0; i < list.Length; i++)
            stringBuilder.Append(list[i]);
        Debug.LogError(stringBuilder.ToString());
    }

    static public void Exception(WLogType type, Exception ex)
    {
        if (logtype != WLogType.all)
        {
            if (logtype == WLogType.none) return;
            else if (logtype != type) return;
        }
        Debug.LogException(ex);
    }
}
