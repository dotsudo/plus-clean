namespace Plus.Communication.ConnectionManager
{
    internal static class GameSocketManagerStatics
    {
        public static readonly int BufferSize = 8192;
        public static readonly int MaxPacketSize = BufferSize - 4;
    }
}