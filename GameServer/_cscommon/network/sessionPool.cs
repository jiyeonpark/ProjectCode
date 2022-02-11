using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace WCS.Network
{

    public class SessionPool
    {
        #region Singleton
        private static SessionPool _instance = null;

        public static SessionPool instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new SessionPool();
                }

                return _instance;
            }
        }
        #endregion
             
        public ConcurrentDictionary<long, Session> _session_wait = null;

        public int MenualBufferSize { get; set; }   //  이거 설정 되면, 기본값으로 안하고 이 크기로 만든다.

        private PoolBase<Session> _pool = null;

        public SessionPool()
        {
            _pool = new PoolBase<Session>();
            _session_wait = new ConcurrentDictionary<long, Session>();
        }

        public void Initialize(int default_size, int alloc_size)
        {
            _pool.Initialize(default_size, alloc_size);
        }

        public Session Pop()
        {
            return _pool.Pop();
        }

        public void Push(Session stream)
        {
            _pool.Push(stream);
        }

        public Session PopWait(long session_id)
        {
            Session session = null;
            _session_wait.TryRemove(session_id, out session);
         
            return session;
        }
        public void PushWait(Session session)
        {
            if (session != null && session.SessionID != 0)
            {
                //일단 10초
                session.Release();
                _session_wait.TryAdd(session.SessionID, session);
                
            }
            else
            {
                logger.Warn($"PushWait session.SessionID {session.SessionID}");
            }
        }

    }

}