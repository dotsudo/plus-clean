namespace Plus.Communication.Packets.Outgoing.Rooms.Action
{
    internal class IgnoreStatusComposer : ServerPacket
    {
        public IgnoreStatusComposer(int status, string username)
            : base(ServerPacketHeader.IgnoreStatusMessageComposer)
        {
            WriteInteger(status);
            WriteString(username);
        }
    }
}