namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class UserNameChangeComposer : ServerPacket
    {
        public UserNameChangeComposer(int roomId, int virtualId, string username)
            : base(ServerPacketHeader.UserNameChangeMessageComposer)
        {
            WriteInteger(roomId);
            WriteInteger(virtualId);
            WriteString(username);
        }
    }
}