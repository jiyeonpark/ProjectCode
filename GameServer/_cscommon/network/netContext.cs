using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WCS.Network
{
	public class netContext
    {
        private ConcurrentStack<Context> _stack = null;
#if SERVER_UNITY
        private List<ConcurrentQueue<Context>> _queue = null;
#else
        private ConcurrentQueue<Context> _queue = null;
#endif
        private readonly int _default_alloc_size = 0;
        private readonly int _alloc_size = 0;
        private int _cur_index = 0;
        //private int _get_index = 1;

        public netContext(int default_size, int create_size)
        {
            _stack = new ConcurrentStack<Context>();
#if SERVER_UNITY
            _queue = new List<ConcurrentQueue<Context>>() { new ConcurrentQueue<Context>(), new ConcurrentQueue<Context>() };
#else
            _queue = new ConcurrentQueue<Context>();
#endif

            _default_alloc_size = default_size;
            _alloc_size = create_size;

            alloc(_default_alloc_size);
        }

        private void alloc(int size)
        {
            for (int i = 0; i < size; i++)
            {
                _stack.Push(new Context());
            }

            WCS.logger.Info($"{size}");
        }

        private Context Pop()
        {
            if (_stack.IsEmpty)
            {
                alloc(_alloc_size);
            }

            Context context = null;
            _stack.TryPop(out context);
            return context;
        }

        public void Push(Context context)
        {
            _stack.Push(context);
        }

        public bool PopQueue(out Context context)
        {
#if SERVER_UNITY
            return _queue[_cur_index].TryDequeue(out context);
#else
            return _queue.TryDequeue(out context);
#endif
        }

        public void PushQueue(string session, ReadStream stream)
        {
            var context = Pop();
            
            context.stream = stream;
            context.stream.webcmd = session;
#if SERVER_UNITY
            _queue[_cur_index].Enqueue(context);
#else
            _queue.Enqueue(context);
#endif
        }
        public void PushQueue(WCS.Network.ClientToken token, ReadStream stream)
        {
            var context = Pop();
            context.token = token;
            context.stream = stream;
#if SERVER_UNITY
            _queue[_cur_index].Enqueue(context);
#else
            _queue.Enqueue(context);
#endif
        }

        public void SwitchIndex()
        {
            //Interlocked.Exchange(ref _cur_index, _cur_index == 0 ? 1 : 0);
            //Interlocked.Exchange(ref _get_index, _get_index == 0 ? 1 : 0);
        }
        public class Context
        {
            //public long SessionID { get; set; }
            public WCS.Network.ClientToken token { get; set; }
            public ReadStream stream { get; set; }
        }
       
    }


#if SERVER_UNITY
    public class rpcContext
    {
        private ConcurrentStack<Context> _stack = null;
        private List<ConcurrentQueue<Context>> _queue = null;
        private readonly int _default_alloc_size = 0;
        private readonly int _alloc_size = 0;
        private int _cur_index = 0;
        //private int _get_index = 1;

        public rpcContext(int default_size, int create_size)
        {
            _stack = new ConcurrentStack<Context>();
            _queue = new List<ConcurrentQueue<Context>>() { new ConcurrentQueue<Context>(), new ConcurrentQueue<Context>() };

            _default_alloc_size = default_size;
            _alloc_size = create_size;

            alloc(_default_alloc_size);
        }

        private void alloc(int size)
        {
            for (int i = 0; i < size; i++)
            {
                _stack.Push(new Context());
            }

            WCS.logger.Info($"{size}");
        }

        private Context Pop()
        {
            if (_stack.IsEmpty)
            {
                alloc(_alloc_size);
            }

            Context context = null;
            _stack.TryPop(out context);
            return context;
        }

        public void Push(Context context)
        {
            _stack.Push(context);
        }

        public bool PopQueue(out Context context)
        {
            return _queue[_cur_index].TryDequeue(out context);
        }

        public void PushQueue(string session, ReadStream stream)
        {
            var context = Pop();

            context.stream = stream;
            context.stream.webcmd = session;
            _queue[_cur_index].Enqueue(context);
        }
        public void PushQueue(WCS.Network.ClientToken session, ReadStream stream)
        {
            var context = Pop();
            context.session = session;
            context.stream = stream;
            _queue[_cur_index].Enqueue(context);
        }

        public void SwitchIndex()
        {
            //Interlocked.Exchange(ref _cur_index, _cur_index == 0 ? 1 : 0);
            //Interlocked.Exchange(ref _get_index, _get_index == 0 ? 1 : 0);
        }
        public class Context
        {
            public WCS.Network.ClientToken session { get; set; }
            public ReadStream stream { get; set; }
        }

    }
#endif
}