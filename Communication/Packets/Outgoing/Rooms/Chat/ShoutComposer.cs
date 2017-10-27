namespace Plus.Communication.Packets.Outgoing.Rooms.Chat
{
    public class ShoutComposer : ServerPacket
    {
        public ShoutComposer(int virtualId, string message, int emotion, int colour)
            : base(ServerPacketHeader.ShoutMessageComposer)
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