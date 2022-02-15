using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CoroutineInfo
{
    public MonoBehaviour mo;
    public Coroutine co;
};

public static class MonoBehaviourExtension
{

    public static Dictionary<string, CoroutineInfo> _dicCoroutine = new Dictionary<string, CoroutineInfo>();

    public static void After(this MonoBehaviour behaviour, float wait, Action f)
    {
        behaviour.StartCoroutine(AfterCR(wait, f));
    }

    public static void After(this MonoBehaviour behaviour, string str, float wait, Action f)
    {
        CoroutineInfo t = new CoroutineInfo();
        t.mo = behaviour;
        t.co = behaviour.StartCoroutine(AfterCR(str, wait, f));
        _dicCoroutine.Add(str, t);
    }

    static IEnumerator AfterCR(float wait, Action f)
    {
        yield return new WaitForSeconds(wait);
        f();
    }

    static IEnumerator AfterCR(string str, float wait, Action f)
    {
        yield return new WaitForSeconds(wait);
        f();
        _dicCoroutine.Remove(str);
    }

    public static void StopCR(this MonoBehaviour behaviour, string str)
    {
        if (_dicCoroutine.ContainsKey(str))
        {
            _dicCoroutine[str].mo.StopCoroutine(_dicCoroutine[str].co);
            _dicCoroutine.Remove(str);
        }
    }

    public static void StopExCoroutine(this MonoBehaviour behaviour)
    {
        foreach (var item in _dicCoroutine)
        {
            item.Value.mo.StopCoroutine(item.Value.co);
        }

        _dicCoroutine.Clear();
    }

    public static void YieldAndExecute(this MonoBehaviour behaviour, Action f)
    {
        behaviour.StartCoroutine(YieldAndExecuteCR(f));
    }

    static IEnumerator YieldAndExecuteCR(Action f)
    {
        yield return null;

        f();
    }

    public static TEnum ToEnum<TEnum>(this string strEnumValue)
    {
        if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
            return default(TEnum);

        return (TEnum)Enum.Parse(typeof(TEnum), strEnumValue);
    }
}
