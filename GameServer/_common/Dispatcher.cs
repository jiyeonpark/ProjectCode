using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace LetsBaseball
{
    public class Dispatcher : MonoBehaviour
    {
        public static int tID = 0;

        public static void RunOnMainThread(Func<UniTask> action)
        {
            _actions.Enqueue(action);
            _queued = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (_instance == null)
            {
                _instance = new GameObject("Dispatcher").AddComponent<Dispatcher>();
                DontDestroyOnLoad(_instance.gameObject);
            }
            tID = System.Threading.Thread.CurrentThread.ManagedThreadId;
            CustomLog.Log(WLogType.debug, "-------------------- Main Thread ID : ", tID.ToString());
        }

        private void Update()
        {
            if (_queued)
            {
                _queued = false;
                Func<UniTask> action = null;
                while (_actions.TryDequeue(out action))
                {
                    action();
                }
            }
        }

        static Dispatcher _instance;
        static volatile bool _queued = false;
        static ConcurrentQueue<Func<UniTask>> _actions = new ConcurrentQueue<Func<UniTask>>();
    }
}