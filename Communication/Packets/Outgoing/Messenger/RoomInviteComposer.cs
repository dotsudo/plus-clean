namespace Plus.Communication.Packets.Outgoing.Messenger
{
    internal class RoomInviteComposer : ServerPacket
    {
        public RoomInviteComposer(int senderId, string text)
            : base(ServerPacketHeader.RoomInviteMessageComposer)
        {
            WriteInteger(senderId);
            WriteString(text);
        }
    }
}