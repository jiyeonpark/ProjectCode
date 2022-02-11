using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

namespace WCS.Network
{
    class SocketAsyncEventArgsPool
    {
        public delegate void EventArgaHandler(object sender, SocketAsyncEventArgs e);

        private static readonly int DEFAULT_EVENTTAG_SIZE = 10;

        private readonly Stack<SocketAsyncEventArgs> _stack = null;
        
        private EventArgaHandler _eventArgaHandler = null;

        public SocketAsyncEventArgsPool(EventArgaHandler eventArgaHandler)
        {
            _stack = new Stack<SocketAsyncEventArgs>();
            _eventArgaHandler = eventArgaHandler;

            for (int i = 0; i < DEFAULT_EVENTTAG_SIZE; i++)
            {
                CreateEventArgs();
            }
        }

        public void Dispose()
        {
            while (0 < _stack.Count)
            {
                SocketAsyncEventArgs item = _stack.Pop();
                if (null != item)
                {
                    item.Dispose();
                }
            }
        }

       

        private void CreateEventArgs()
        {
            var temp = new SocketAsyncEventArgs();
            temp.Completed += new EventHandler<SocketAsyncEventArgs>(this._eventArgaHandler);
            temp.UserToken = null;
            _stack.Push(temp);
        }

        public SocketAsyncEventArgs Pop()
        {
            lock (_stack)
            {
                if (0 == _stack.Count)
                {
                    CreateEventArgs();
                }

                SocketAsyncEventArgs item = _stack.Pop();
                return item;
            }
        }

        public void Push(SocketAsyncEventArgs item)
        {
            if (null == item)
            {
                throw new ArgumentNullException("item is null.");
            }

            lock (_stack)
            {
                _stack.Push(item);
            }
        }
    }   

    public class SocketAsyncEventArgsPair
    {
        private SocketAsyncEventArgs _event_recv = null;
        private SocketAsyncEventArgs _event_send = null;

        public SocketAsyncEventArgs RecvEventArgs
        {
            get { return _event_recv; }
            set { _event_recv = value; }
        }

        public SocketAsyncEventArgs SendEventArgs
        {
            get { return _event_send; }
            set { _event_send = value; }
        }
    }

    class SocketAsyncEventArgsPairPool
    {
        private readonly ConcurrentStack<SocketAsyncEventArgsPair> _stack = null;
        private Int32 nextTokenId = 0;
        public SocketAsyncEventArgsPairPool()
        {
            _stack = new ConcurrentStack<SocketAsyncEventArgsPair>();
        }
        internal Int32 AssignTokenId()
        {
            Int32 tokenId = Interlocked.Increment(ref nextTokenId);
            return tokenId;
        }
        public void Dispose()
        {
            while (false == _stack.IsEmpty)
            {
                SocketAsyncEventArgsPair item;
                if (_stack.TryPop(out item))
                {
                    item.RecvEventArgs.Dispose();
                    item.SendEventArgs.Dispose();
                }
            }
        }

        public int Count
        {
            get { return _stack.Count; }
        }

        public SocketAsyncEventArgsPair Pop()
        {
            SocketAsyncEventArgsPair item = null;
            _stack.TryPop(out item);
            return item;
        }

        public void Push(SocketAsyncEventArgsPair item)
        {
            if (null == item)
            {
                throw new ArgumentNullException("item is null.");
            }

            _stack.Push(item);
        }
    } 
}