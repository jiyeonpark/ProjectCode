#if SERVER_UNITY

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace WCS.Network
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class wp_rpc_base : wp_base
    {
        public ushort len;
        public ushort command;
        public int error_code;
        public wp_rpc_base(wre_cmd cmd)
        {
            command = (ushort)cmd;
        }

        protected void Serialize(BinaryWriter bw)
        {
            bw.Write(len);
            bw.Write(command);
            bw.Write(error_code);
        }

        protected void Deserialize(BinaryReader br)
        {
            error_code = br.ReadInt32();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class wp_gm_base : wp_base
    {
        public ushort len;
        public ushort command;
        public int error_code;
        public wp_gm_base(wge_cmd cmd)
        {
            command = (ushort)cmd;
        }

        protected void Serialize(BinaryWriter bw)
        {
            bw.Write(len);
            bw.Write(command);
            bw.Write(error_code);
        }

        protected void Deserialize(BinaryReader br)
        {
            error_code = br.ReadInt32();
        }
    }
}

#endif