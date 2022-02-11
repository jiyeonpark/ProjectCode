using System;
using System.Runtime.CompilerServices;
using LetsBaseball;
using Cysharp.Threading.Tasks;

namespace WCS
{
    public static class logger
    {
        public static string ToNormalString(this DateTime time) => time.ToString("MMdd HH:mm:ss.ffffff");

        public static void Info(string message, [CallerFilePath] string path = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = -1)
        {
            int tID = System.Threading.Thread.CurrentThread.ManagedThreadId;
            if (tID != Dispatcher.tID)
            {
                Dispatcher.RunOnMainThread(async () =>
                {
                    await UniTask.Yield();
                    CustomLog.Log(WLogType.debug, "I", DateTime.Now.ToNormalString(), " : ", tID.ToString(), path.Substring(path.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1), ":", line.ToString(), "]", member, "() - ", message);
                });
            }
            else
                CustomLog.Log(WLogType.debug, "I", DateTime.Now.ToNormalString(), " : ", tID.ToString(), path.Substring(path.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1), ":", line.ToString(), "]", member, "() - ", message);
        }

        public static void Debug(string message, [CallerFilePath] string path = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = -1)
        {
            int tID = System.Threading.Thread.CurrentThread.ManagedThreadId;
            if (tID != Dispatcher.tID)
            {
                Dispatcher.RunOnMainThread(async () =>
                {
                    await UniTask.Yield();
                    CustomLog.Log(WLogType.debug, "D", DateTime.Now.ToNormalString(), " : ", tID.ToString(), path.Substring(path.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1), ":", line.ToString(), "]", member, "() - ", message);
                });
            }
            else
                CustomLog.Log(WLogType.debug, "D", DateTime.Now.ToNormalString(), " : ", tID.ToString(), path.Substring(path.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1), ":", line.ToString(), "]", member, "() - ", message);
        }

        public static void Warn(string message, [CallerFilePath] string path = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = -1)
        {
            int tID = System.Threading.Thread.CurrentThread.ManagedThreadId;
            if (tID != Dispatcher.tID)
            {
                Dispatcher.RunOnMainThread(async () =>
                {
                    await UniTask.Yield();
                    CustomLog.Warning(WLogType.debug, "W", DateTime.Now.ToNormalString(), " : ", tID.ToString(), path.Substring(path.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1), ":", line.ToString(), "]", member, "() - ", message);
                });
            }
            else
                CustomLog.Warning(WLogType.debug, "W", DateTime.Now.ToNormalString(), " : ", tID.ToString(), path.Substring(path.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1), ":", line.ToString(), "]", member, "() - ", message);
        }

        public static void Error(string message, [CallerFilePath] string path = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = -1)
        {
            int tID = System.Threading.Thread.CurrentThread.ManagedThreadId;
            if (tID != Dispatcher.tID)
            {
                Dispatcher.RunOnMainThread(async () =>
                {
                    await UniTask.Yield();
                    CustomLog.Error(WLogType.debug, "E", DateTime.Now.ToNormalString(), " : ", tID.ToString(), path.Substring(path.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1), ":", line.ToString(), "]", member, "() - ", message);
                });
            }
            else
                CustomLog.Error(WLogType.debug, "E", DateTime.Now.ToNormalString(), " : ", tID.ToString(), path.Substring(path.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1), ":", line.ToString(), "]", member, "() - ", message);
        }
    }

    
}
