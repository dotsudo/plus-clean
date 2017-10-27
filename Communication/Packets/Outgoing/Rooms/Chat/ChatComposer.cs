namespace Plus.Communication.Packets.Outgoing.Rooms.Chat
{
    public class ChatComposer : ServerPacket
    {
        public ChatComposer(int virtualId, string message, int emotion, int colour)
            : base(ServerPacketHeader.ChatMessageComposer)
        {
            WriteInteger(virtualId);
            WriteString(message);
            WriteInteger(emotion);
            WriteInteger(colour);
            WriteInteger(0);
            WriteInteger(-1);
        }
    }
}