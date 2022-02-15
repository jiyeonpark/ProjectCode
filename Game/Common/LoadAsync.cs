using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface LoadAsync
{
    public void Init(ResourceState state, Action action);
    public void OnDone();
    public bool IsDone();
}

public class LoadBehaviour : MonoBehaviour, LoadAsync
{
    bool isDone = false;

    protected ResourceState resourceState = ResourceState.None;

    public virtual void Init(ResourceState state, Action action) { isDone = false; resourceState = state; LoadAddressable(() => { OnDone(); OnCompleted(); action.Invoke(); }); }
    public virtual async UniTask LoadAddressable(Action action) { }

    public virtual void OnCompleted() { }

    public void OnDone() { isDone = true; }
    public bool IsDone() { return isDone; }
}

public class LoadScriptable : ScriptableObject, LoadAsync
{
    bool isDone = false;

    protected ResourceState resourceState = ResourceState.None;

    public virtual void Init(ResourceState state, Action action) { isDone = false; resourceState = state; LoadAddressable(() => { OnDone(); OnCompleted(); action.Invoke(); }); }
    public virtual async UniTask LoadAddressable(Action action) { }

    public virtual void OnCompleted() { }

    public void OnDone() { isDone = true; }
    public bool IsDone() { return isDone; }
}
