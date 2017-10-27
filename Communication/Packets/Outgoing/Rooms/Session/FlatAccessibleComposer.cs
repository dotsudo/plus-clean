namespace Plus.Communication.Packets.Outgoing.Rooms.Session
{
    internal class FlatAccessibleComposer : ServerPacket
    {
        public FlatAccessibleComposer(string username)
            : base(ServerPacketHeader.FlatAccessibleMessageComposer)
        {
            WriteString(username);
        }
    }
}