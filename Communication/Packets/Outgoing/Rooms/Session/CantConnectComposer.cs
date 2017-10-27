namespace Plus.Communication.Packets.Outgoing.Rooms.Session
{
    internal class CantConnectComposer : ServerPacket
    {
        public CantConnectComposer(int error)
            : base(ServerPacketHeader.CantConnectMessageComposer)
        {
            WriteInteger(error);
        }
    }
}