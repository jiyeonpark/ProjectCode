namespace WCS.Network
{
    public class NET_define
    {
        public const int EFFECTIVE_ETHERNET_PACKET_SIZE = 1460;

        public static readonly int LISTEN_WAIT_MSEC = 1000;
        public static readonly int LISTEN_BACKLOG_SIZE = 100;        
        public static readonly int POOL_SENDBUFFER_DEFAULT = 10;        // session 당
        public static readonly int POOL_SENDBUFFER_ADDCREATE = 32;
        public static readonly int MAX_PACKET_STACK_SIZE = 5;
        public static readonly int ASYNC_EVENT_BUFFER_SIZE = 200000;        
        public static readonly int PACKET_BUFFER_SIZE = 8192 * 2;
        public static readonly int FILEDATA_BUFFER_SIZE = PACKET_BUFFER_SIZE - 128;
        public static readonly int PACKET_SEND_BUFFER_SIZE = 8192 * 10;        
        public static readonly int PACKET_LENGTH_SIZE = 2;
        public static readonly int PACKET_COMMAND_SIZE = 2;
        public static readonly int PACKET_HEADER_SIZE = PACKET_LENGTH_SIZE + PACKET_COMMAND_SIZE;
    }
}
