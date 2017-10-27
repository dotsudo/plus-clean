namespace Plus.Communication.Packets.Outgoing.Rooms.Chat
{
    public class UserTypingComposer : ServerPacket
    {
        public UserTypingComposer(int virtualId, bool typing)
            : base(ServerPacketHeader.UserTypingMessageComposer)
        {
            WriteInteger(virtualId);
            WriteInteger(typing ? 1 : 0);
        }
    }
}