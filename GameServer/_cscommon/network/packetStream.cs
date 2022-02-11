using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;

namespace WCS.Network
{
    public class ReadStreamPool
    {
        #region Singleton
        private static ReadStreamPool _instance = null;

        public static ReadStreamPool instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new ReadStreamPool();
                }

                return _instance;
            }
        }
        #endregion

        public int MenualBufferSize { get; set; }   //  이거 설정 되면, 기본값으로 안하고 이 크기로 만든다.

        private PoolBase<ReadStream> _pool = null;

        public ReadStreamPool()
        {
            _pool = new PoolBase<ReadStream>();
        }

        public void Initialize(int default_size, int alloc_size)
        {
            _pool.Initialize(default_size, alloc_size);
        }

        public ReadStream Pop()
        {
            return _pool.Pop();
        }

        public void Push(ReadStream stream)
        {
            _pool.Push(stream);
        }
    }

    public class SendStreamPool
    {
        #region Singleton
        private static SendStreamPool _instance = null;

        public static SendStreamPool instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new SendStreamPool();
                }

                return _instance;
            }
        }
        #endregion

        public int MenualBufferSize { get; set; }   //  이거 설정 되면, 기본값으로 안하고 이 크기로 만든다.

        private PoolBase<SendStream> _pool = null;

        public SendStreamPool()
        {
            _pool = new PoolBase<SendStream>();

        }

        public void Initialize(int default_size, int alloc_size)
        {
            _pool.Initialize(default_size, alloc_size);
        }

        public SendStream Pop()
        {
            return _pool.Pop();
        }

        public void Push(SendStream stream)
        {
            _pool.Push(stream);
        }
        //public SendStream CreateSendStream(byte[] buffer, int size)
        //{
        //    var stream = Pop();


        //    stream.bw.Write(buffer, 0, size);

        //    //packet.Serialize(stream.bw);

        //    // 패킷 크기 설정
        //   // ushort len = (ushort)stream.position;
        //   // stream.position = 0;
        //   // stream.bw.Write(len);
        //   // stream.position = len;

        //    return stream;
        //}
        public SendStream CreateSendStream<T>(T packet) where T : class, ISerialize
        {
            var stream = Pop();
            packet.Serialize(stream.bw);

            // 패킷 크기 설정
            ushort len = (ushort)stream.position;
            stream.position = 0;
            stream.bw.Write(len);
            stream.position = len;

            return stream;
        }
    }

    public class PacketStream : IResetable
    {
        public byte[] buffer { get; set; }
        public string webcmd { get; set; }
        public ushort command { get; set; }
        public int position { get { return (int)ms.Position; } set { ms.Position = value; } }

        protected MemoryStream ms = null;

        public PacketStream(bool writable, int size)
        {
            buffer = new byte[size];
            ms = new MemoryStream(buffer, writable);
        }

        public void Reset()
        {
            position = 0;
            Array.Clear(buffer, 0x0, buffer.Length);
        }
    }

    public class ReadStream : PacketStream
    {
        public BinaryReader br { get; private set; }

        public ReadStream() : base(true, (0 < ReadStreamPool.instance.MenualBufferSize) ? ReadStreamPool.instance.MenualBufferSize : NET_define.PACKET_BUFFER_SIZE)
        {
            br = new BinaryReader(base.ms);
        }

        //public void Set(byte[] data, int pos, int size_)
        //{
        //    if (NET_define.PACKET_BUFFER_SIZE > size_)
        //    {
        //        //command = BitConverter.ToUInt16(data, pos + NET_define.PACKET_LENGTH_SIZE);
        //        //Array.Copy(data, pos + NET_define.PACKET_HEADER_SIZE, buffer, 0, size_ - NET_define.PACKET_HEADER_SIZE);
                
        //        Array.Copy(data, pos , buffer, 0, size_ );
        //    }
        //}
        public void Set(byte[] data, int pos, int size_)
        {
            if (NET_define.PACKET_BUFFER_SIZE > size_)
            {
                command = BitConverter.ToUInt16(data, pos + NET_define.PACKET_LENGTH_SIZE);

                Array.Copy(data, pos + NET_define.PACKET_HEADER_SIZE, buffer, 0, size_ - NET_define.PACKET_HEADER_SIZE);
            }
        }
        //public void Set(ushort cmd, byte[] data, int pos, int size_)
        //{
        //    if (NET_define.PACKET_BUFFER_SIZE > size_)
        //    {
        //        command = cmd;
        //        Array.Copy(data, pos, buffer, 0, size_);
        //    }
        //}
    }

    public class SendStream : PacketStream
    {
        public BinaryWriter bw { get; private set; }

        public SendStream() : base(true, (0 < SendStreamPool.instance.MenualBufferSize) ? SendStreamPool.instance.MenualBufferSize : NET_define.PACKET_BUFFER_SIZE)
        {
            bw = new BinaryWriter(base.ms);
        }
    }
}
