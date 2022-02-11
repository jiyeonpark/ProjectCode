
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using WCS.Util;
using System.Collections.Concurrent;

namespace WCS.Network
{
    
    public class Heartbeat
    {
        public Heartbeat()
        {
            mSessionID = 0;
            mIntervalInSeconds = 1;
            mNextHeartbeatTime = 0;
            mProtocol = 0;
        }

        public void NextHeartbeat() { mNextHeartbeatTime = DateTime.Now.ToUnixTimeInt() +  mIntervalInSeconds; }
        public bool CanPump()  { return (DateTime.Now.ToUnixTimeInt() > mNextHeartbeatTime); }
        
        public long mSessionID;
        public string mXmlFileName;
        public string mXml;
        public InGameState mProtocol;
        public string mListenerName;
        public int mIntervalInSeconds;
        public int mNextHeartbeatTime;
    }

    public class HeartbeatTask
    {
        public string mCommand = "";
        public List<string> mArgs = new List<string>();
    }

    public class HeartbeatManager
    {
        
        public List<Heartbeat> mHeartbeats;
        public string mIniFileName;
        public int mIniReloadIntervalInSeconds;
        public int mIntervalInSeconds;
        public int mNextAllowableExecuteTime;
        public int mIniTimer;
        public HeartbeatManager()
        {
            mIniTimer= 0;
            mIntervalInSeconds = 1;
            mIniReloadIntervalInSeconds = 5;
            mNextAllowableExecuteTime = 0;
            mHeartbeats = new List<Heartbeat>();
        }
        //public HeartbeatManager(string cronFileName, int cronReloadIntervalInSeconds = 60)
        //{
        //    mIniTimer = 0;
        //    mIniFileName = cronFileName;
        //    mIniReloadIntervalInSeconds = cronReloadIntervalInSeconds;
        //    mNextAllowableExecuteTime = 0;
        //}
        
        public int ProcessTask(bool isBackGround,int now, InGameState state,  ref ConcurrentQueue<HeartbeatTask> tasks )
        {
            
            if (mIniTimer < now)
            {                
                mHeartbeats.Clear();

                string hbString = "Heartbeat";

                //for (int i = 1; i <= 10; ++i)
                {
                    Heartbeat hb = new Heartbeat();

                    hb.mProtocol = state;
                    hb.mXmlFileName = "";
                    hb.mListenerName = hbString;
                    hb.mIntervalInSeconds = mIntervalInSeconds;
                    hb.NextHeartbeat();
                    mHeartbeats.Add(hb);
                }
               
                mIniTimer = now + mIniReloadIntervalInSeconds;

               
            }

            foreach(var hb in mHeartbeats)
            {               
                if (hb.CanPump())
                {
                    HeartbeatTask task = new HeartbeatTask();
                    task.mCommand = hb.mProtocol.ToString();
                    task.mArgs.Add(hb.mListenerName);
                    tasks.Enqueue(task);
                    //hb.NextHeartbeat();
                    
                }

            }

            //if (isBackGround)
            //{
            //    mHeartbeats.Clear();
            //}
            return tasks.Count;
        }

        public void SetIniReloadIntervalInSeconds(int interval) { mIniReloadIntervalInSeconds = interval; }
        public int GetIniReloadIntervalInSeconds() { return mIniReloadIntervalInSeconds; }
        public void SetIniFileName(string fileName) { mIniFileName = fileName; }
        public string GetIniFileName() { return mIniFileName; }

        public void SetIntervalInSeconds(int interval) { mIntervalInSeconds = interval; }
        
    }
}