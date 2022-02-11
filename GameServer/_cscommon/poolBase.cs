using System.Collections.Concurrent;

namespace WCS
{
    public interface IResetable
    {
        void Reset();
    }

    public class PoolBase<T> where T : class, IResetable, new()
    {
        private int _default_alloc_size;
        private int _alloc_size;

        private ConcurrentStack<T> _stack = null;
        private bool _initialize_complete = false;

        public void Initialize(int default_size, int alloc_size)
        {
            if (false == _initialize_complete)
            {
                _initialize_complete = true;

                _default_alloc_size = default_size;
                _alloc_size = alloc_size;

                _stack = new ConcurrentStack<T>();

                alloc(_default_alloc_size);
            }
        }

        public T Pop()
        {
            lock(_stack)
            {
                if (_stack.IsEmpty)
                {
                    alloc(_alloc_size);
                }

                T item = null;
                _stack.TryPop(out item);
                item.Reset();
                return item;
            }
        }

        public void Push(T item)
        {
            //item.Reset();

            _stack.Push(item);
        }

        private void alloc(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _stack.Push(new T());
            }
        }
    }
}

