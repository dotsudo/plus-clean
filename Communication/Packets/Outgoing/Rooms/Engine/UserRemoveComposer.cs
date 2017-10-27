namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class UserRemoveComposer : ServerPacket
    {
        public UserRemoveComposer(int id)
            : base(ServerPacketHeader.UserRemoveMessageComposer)
        {
            WriteString(id.ToString());
        }
    }
}