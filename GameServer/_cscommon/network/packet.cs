using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace WCS.Network
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class wp_base
    {
        public wp_base()
        {
        }

        public void SerializeList<T>(BinaryWriter writer, List<T> list) where T : ISerialize
        {
            if (list != null)
            {
                writer.Write((ushort)list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Serialize(writer);
                }
            }
            else
                writer.Write((ushort)0);
        }

        public void SerializeList(BinaryWriter writer, List<float> list)
        {
            if (list != null)
            {
                writer.Write((ushort)list.Count);
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        writer.Write(list[i]);
                    }
                }
            }
            else
                writer.Write((ushort)0);
        }
        public void SerializeList(BinaryWriter writer, List<short> list)
        {
            if (list != null)
            {
                writer.Write((ushort)list.Count);
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        writer.Write(list[i]);
                    }
                }
            }
            else
                writer.Write((ushort)0);
        }

        public void SerializeList(BinaryWriter writer, List<int> list)
        {
            if (list != null)
            {
                writer.Write((ushort)list.Count);
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        writer.Write(list[i]);
                    }
                }
            }
            else
                writer.Write((ushort)0);
        }
        public void SerializeList(BinaryWriter writer, List<long> list)
        {
            if (list != null)
            {
                writer.Write((ushort)list.Count);
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        writer.Write(list[i]);
                    }
                }
            }
            else
                writer.Write((ushort)0);
        }

        public void SerializeList(BinaryWriter writer, List<ushort> list)
        {
            if (list != null)
            {
                writer.Write((ushort)list.Count);
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        writer.Write(list[i]);
                    }
                }
            }
            else
                writer.Write((ushort)0);
        }

        public void SerializeList(BinaryWriter writer, List<uint> list)
        {
            if (list != null)
            {
                writer.Write((ushort)list.Count);
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        writer.Write(list[i]);
                    }
                }
            }
            else
                writer.Write((ushort)0);
        }

        public void SerializeList(BinaryWriter writer, List<ulong> list)
        {
            if (list != null)
            {
                writer.Write((ushort)list.Count);
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        writer.Write(list[i]);
                    }
                }
            }
            else
                writer.Write((ushort)0);
        }

        public void SerializeList(BinaryWriter writer, List<byte> list)
        {
            if (list != null)
            {
                writer.Write((ushort)list.Count);
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        writer.Write(list[i]);
                    }
                }
            }
            else
                writer.Write((ushort)0);
        }

        public void SerializeListEnum<T>(BinaryWriter writer, List<T> list)
        {
            if (list != null)
            {
                writer.Write((ushort)list.Count);
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var value = Convert.ToInt32(list[i]);   // boxing ?   체크 좀
                        writer.Write(value);
                    }
                }
            }
            else
                writer.Write((ushort)0);
        }

        public void SerializeList(BinaryWriter writer, List<byte[]> list)
        {
            if (list != null)
            {
                writer.Write((ushort)list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    var value = list[i];
                    WriteBytes(writer, value);
                }
            }
            else
                writer.Write((ushort)0);
        }

        public void DeserializeList(BinaryReader reader, List<byte> list)
        {
            var count = reader.ReadUInt16();

            if (0 < count)
            {
                list = new List<byte>(count);
                for (int i = 0; i < count; i++)
                {
                    list.Add(reader.ReadByte());
                }
            }
            else
            {
                list = null;
            }
        }
        public void DeserializeList(BinaryReader reader, List<int> list)
        {
            var count = reader.ReadUInt16();

            if (0 < count)
            {
                list = new List<int>(count);
                for (int i = 0; i < count; i++)
                {
                    list.Add(reader.ReadInt32());
                }
            }
            else
            {
                list = null;
            }
        }

        public void DeserializeList(BinaryReader reader, List<short> list)
        {
            var count = reader.ReadUInt16();

            if (0 < count)
            {
                list = new List<short>(count);
                for (int i = 0; i < count; i++)
                {
                    list.Add(reader.ReadInt16());
                }
            }
            else
            {
                list = null;
            }
        }

        public void DeserializeList(BinaryReader reader, out List<long> list)
        {
            var count = reader.ReadUInt16();

            if (0 < count)
            {
                list = new List<long>(count);
                for (int i = 0; i < count; i++)
                {
                    list.Add(reader.ReadInt64());
                }
            }
            else
            {
                list = null;
            }
        }

        public void DeserializeList(BinaryReader reader, out List<uint> list)
        {
            var count = reader.ReadUInt16();

            if (0 < count)
            {
                list = new List<uint>(count);
                for (int i = 0; i < count; i++)
                {
                    list.Add(reader.ReadUInt32());
                }
            }
            else
            {
                list = null;
            }
        }

        public void DeserializeList(BinaryReader reader, out List<ushort> list)
        {
            var count = reader.ReadUInt16();

            if (0 < count)
            {
                list = new List<ushort>(count);
                for (int i = 0; i < count; i++)
                {
                    list.Add(reader.ReadUInt16());
                }
            }
            else
            {
                list = null;
            }
        }

        public List<float> floatDeserializeList(BinaryReader reader)
        {

            var count = reader.ReadUInt16();
            List<float> list = null;

            if (0 < count)
            {
                list = new List<float>(count);

                for (int i = 0; i < count; i++)
                {
                    list.Add(reader.ReadSingle());
                }
            }

            return list;
        }
        public List<uint> uintDeserializeList(BinaryReader reader)
        {

            var count = reader.ReadUInt16();
            List<uint> list = null;

            if (0 < count)
            {
                list = new List<uint>(count);

                for (int i = 0; i < count; i++)
                {
                    list.Add(reader.ReadUInt32());
                }
            }

            return list;
        }
        public List<int> intDeserializeList(BinaryReader reader)
        {

            var count = reader.ReadUInt16();
            List<int> list = null;

            if (0 < count)
            {
                list = new List<int>(count);

                for (int i = 0; i < count; i++)
                {
                    list.Add(reader.ReadInt32());
                }
            }

            return list;
        }
        public List<short> shortDeserializeList(BinaryReader reader)
        {

            var count = reader.ReadUInt16();
            List<short> list = null;

            if (0 < count)
            {
                list = new List<short>(count);

                for (int i = 0; i < count; i++)
                {
                    list.Add(reader.ReadInt16());
                }
            }

            return list;
        }
        public List<ushort> ushortDeserializeList(BinaryReader reader)
        {

            var count = reader.ReadUInt16();
            List<ushort> list = null;

            if (0 < count)
            {
                list = new List<ushort>(count);

                for (int i = 0; i < count; i++)
                {
                    list.Add(reader.ReadUInt16());
                }
            }

            return list;
        }

        public List<long> longDeserializeList(BinaryReader reader)
        {

            var count = reader.ReadUInt16();
            List<long> list = null;

            if (0 < count)
            {
                list = new List<long>(count);

                for (int i = 0; i < count; i++)
                {
                    list.Add(reader.ReadInt64());
                }
            }

            return list;
        }
        public List<ulong> ulongDeserializeList(BinaryReader reader)
        {

            var count = reader.ReadUInt16();
            List<ulong> list = null;

            if (0 < count)
            {
                list = new List<ulong>(count);

                for (int i = 0; i < count; i++)
                {
                    list.Add(reader.ReadUInt64());
                }
            }

            return list;
        }

        public List<bool> boolDeserializeList(BinaryReader reader)
        {
            var count = reader.ReadUInt16();
            List<bool> list = null;

            if (0 < count)
            {
                list = new List<bool>(count);

                for (int i = 0; i < count; i++)
                {
                    list.Add(reader.ReadBoolean());
                }
            }

            return list;
        }

        public List<byte> byteDeserializeList(BinaryReader reader)
        {

            var count = reader.ReadUInt16();
            List<byte> list = null;

            if (0 < count)
            {
                list = new List<byte>(count);

                for (int i = 0; i < count; i++)
                {
                    list.Add(reader.ReadByte());
                }
            }

            return list;

        }

        //public void DeserializeListEnum<T>(BinaryReader reader, out List<T> list) //???? how dont use box unbox??
        //{
        //    var count = reader.ReadUInt16();

        //    if (0 < count)
        //    {
        //        list = new List<T>(count);
        //        for (int i = 0; i < count; i++)
        //        {
        //            list.Add((T)(object)reader.ReadInt32());        // box, unbox   체크 점
        //        }
        //    }
        //    else
        //    {
        //        list = null;
        //    }
        //}


        public void DeserializeList(BinaryReader reader, out List<byte[]> list)
        {
            var count = reader.ReadUInt16();

            if (0 < count)
            {
                list = new List<byte[]>(count);

                for (int i = 0; i < count; i++)
                {
                    list.Add(ReadBytes(reader));
                }
            }
            else
            {
                list = null;
            }
        }

        public List<T> DeserializeList<T>(BinaryReader reader) where T : wp_base, IDeserialize, new()
        {
            var count = reader.ReadUInt16();
            List<T> list = null;

            if (0 < count)
            {
                list = new List<T>(count);

                for (int i = 0; i < count; i++)
                {
                    T item = new T();
                    item.Deserialize(reader);
                    list.Add(item);
                }
            }

            return list;
        }

        public void WriteBytes(BinaryWriter writer, byte[] bytes)
        {
            if (bytes != null && bytes.Length != 0)
            {
                writer.Write((ushort)bytes.Length);
                writer.Write(bytes);
            }
            else
                writer.Write((ushort)0);
        }

        public void WriteBigBytes(BinaryWriter writer, byte[] bytes)
        {
            if (bytes != null && bytes.Length != 0)
            {
                writer.Write((int)bytes.Length);
                writer.Write(bytes);
            }
            else
                writer.Write((int)0);
        }

        public byte[] ReadBytes(BinaryReader reader)
        {
            var length = reader.ReadUInt16();
            byte[] bytes = null;

            if (length > 0)
            {
                //bytes = new Byte[length];
                //for (int i = 0; i < length; i++)
                //{
                //    bytes[i] = new Byte();
                //    bytes[i] = reader.ReadByte();
                //}
                bytes = reader.ReadBytes((int)length);
            }

            return bytes;
        }

        public byte[] ReadBigBytes(BinaryReader reader)
        {
            var length = reader.ReadUInt32();
            byte[] bytes = null;

            if (length > 0)
            {
                //bytes = new Byte[length];
                //for (int i = 0; i < length; i++)
                //{
                //    bytes[i] = new Byte();
                //    bytes[i] = reader.ReadByte();
                //}
                bytes = reader.ReadBytes((int)length);
            }

            return bytes;
        }

        public void SerializeListString(BinaryWriter writer, List<string> list)
        {
            if (list != null)
            {
                writer.Write((ushort)list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    writer.Write(list[i]);
                }
            }
            else
                writer.Write((ushort)0);
        }

        public List<string> DeserializeListString(BinaryReader reader)
        {
            var count = reader.ReadUInt16();

            List<string> list = null;

            if (0 < count)
            {
                list = new List<string>(count);

                for (int i = 0; i < count; i++)
                {
                    list.Add(reader.ReadString());
                }
            }

            return list;
        }
    }

    public interface ISerialize
    {
        void Serialize(BinaryWriter bw);
    }

    public interface IDeserialize
    {
        void Deserialize(BinaryReader br);
    }

    public class wps_base : wp_base
    {
        public wps_base() { }
        protected void Serialize(BinaryWriter bw) { }
        protected void Deserialize(BinaryReader br) { }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class wp_web_base : wp_base
    {
        public byte flag;
        public int result;

        protected void Serialize(BinaryWriter bw)
        {
            bw.Write(flag);
            bw.Write(result);
        }

        protected void Deserialize(BinaryReader br)
        {
            flag = br.ReadByte();
            result = br.ReadInt32();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class wp_game_base : wp_base
    {
        public ushort len;
        public ushort command;
        public byte flag;
        public wp_game_base(wce_cmd cmd)
        {
            command = (ushort)cmd;
        }

        protected void Serialize(BinaryWriter bw)
        {
            bw.Write(len);
            bw.Write(command);
            bw.Write(flag);
        }

        protected void Deserialize(BinaryReader br)
        {
            flag = br.ReadByte();
        }
    }
}
