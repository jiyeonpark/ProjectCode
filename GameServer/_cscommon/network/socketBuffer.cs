using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;

namespace WCS.Network
{
    class ReadBuffer
    {
        private byte[] _buffer = null;      
        private int _pos = 0;
        private int _start_pos = 0;
        private int _packet_size = 0;
        private int _max_buffser_size = 0;

        public ReadBuffer()
        {
            _max_buffser_size = NET_define.PACKET_BUFFER_SIZE * 10;
            _buffer = new byte[_max_buffser_size];
        }

        public bool Complete(SocketAsyncEventArgs e, out List<ReadStream> list)
        {
            list = null;

            if (_max_buffser_size > _pos + e.BytesTransferred)
            {
                Array.Copy(e.Buffer, e.Offset, _buffer, _pos, e.BytesTransferred);

                _pos += e.BytesTransferred;

                _start_pos = 0;

                while (0 < _pos)
                {
                    _packet_size = BitConverter.ToUInt16(_buffer, _start_pos);

                    if (0 < _packet_size && _packet_size <= _pos)
                    {
                        var stream = ReadStreamPool.instance.Pop();

                        stream.Set(_buffer, _start_pos, _packet_size);

                        if (null == list)
                            list = new List<ReadStream>();

                        list.Add(stream);

                        _pos -= _packet_size;
                        _start_pos += _packet_size;
                    }
                    else
                    {
                        break;
                    }
                }

                if (0 < _pos)
                {
                    Array.Copy(_buffer, _start_pos, _buffer, 0, _pos);
                }

                return true;
            }

            throw new Exception("ReadBuffer() buffer size over");
        }

        public void Reset()
        {
            _pos = 0;
        }
    }

    public class SendBufferPool
    {
        private ConcurrentStack<SendBuffer> _stack = null;
        private int _createSize = 0;

        public void Initialize(int max_session)
        {
            _stack = new ConcurrentStack<SendBuffer>();
            _createSize = NET_define.POOL_SENDBUFFER_ADDCREATE;

            Alloc(max_session * NET_define.POOL_SENDBUFFER_DEFAULT);
        }

        private void Alloc(int size)
        {
            for (int i = 0; i < size; i++)
            {
                _stack.Push(new SendBuffer());
            }
        }

        public SendBuffer pop()
        {
            if (_stack.IsEmpty)
            {
                Alloc(_createSize);
            }

            SendBuffer item = null;
            _stack.TryPop(out item);
            return item;
        }

        public void push(SendBuffer item)
        {
            _stack.Push(item);
        }
    }

    public class SendBuffer
    {
        public byte[] buffer { get; private set; }

        private int _read_pos = 0;
        private int _write_pos = 0;
        private int _data_size = 0;
        private object _lock_object = null;

        public int GetDataSize { get { return _data_size; } }

        public SendBuffer()
        {
            buffer = new byte[NET_define.PACKET_SEND_BUFFER_SIZE];
            _lock_object = new object();
        }

        public void Reset()
        {
            _read_pos = 0;
            _write_pos = 0;
            _data_size = 0;
        }

        public bool Set(SendStream stream)
        {
            return true;
        }

        public bool Set(byte[] data, int size_)
        {
            lock (_lock_object)
            {
                if (0 == size_ || size_ > NET_define.PACKET_BUFFER_SIZE)
                {
                    return true;
                }

                // buffer overflow
                if (NET_define.PACKET_SEND_BUFFER_SIZE < _data_size + size_)
                {
                    return false;
                }

                // 데이터가 앞/뒤 에 나눠진다.
                if (NET_define.PACKET_SEND_BUFFER_SIZE <= _write_pos + size_)
                {
                    int part = NET_define.PACKET_SEND_BUFFER_SIZE - _write_pos;
                    int remain = size_ - part;

                    Array.Copy(data, 0, buffer, _write_pos, part);

                    if (remain > 0)
                    {
                        Array.Copy(data, part, buffer, 0, remain);
                    }

                    _write_pos = remain;

                    if (_write_pos < 0)
                    {
                        _write_pos = 0;
                    }
                }
                else
                {
                    Array.Copy(data, 0, buffer, _write_pos, size_);;
                    _write_pos += size_;
                }

                _data_size += size_;

                return true;
            }
        }

        public int GetSendBuffer(out int read_pos, int max_size = NET_define.EFFECTIVE_ETHERNET_PACKET_SIZE)
        {
            lock (_lock_object)
            {
                read_pos = 0;

                if (0 == _data_size)
                {
                    return 0;
                }

                int buffer_size = 0;

                read_pos = _read_pos;

                if (_write_pos > _read_pos)
                {
                    buffer_size = (_write_pos - _read_pos > max_size) ? (max_size) : (_write_pos - _read_pos);
                    _read_pos += buffer_size;
                }
                // 데이터가 앞뒤로 잘려 들어가 있다.
                else
                {
                    if (NET_define.PACKET_SEND_BUFFER_SIZE - _read_pos > max_size)
                    {
                        buffer_size = max_size;
                        _read_pos += max_size;
                    }
                    else
                    {
                        buffer_size = NET_define.PACKET_SEND_BUFFER_SIZE - _read_pos;
                        _read_pos = 0;
                    }
                }

                _data_size -= buffer_size;

                return buffer_size;
            }
        }
    }
}
