namespace Plus.Communication.Packets.Outgoing.Rooms.Chat
{
    public class WhisperComposer : ServerPacket
    {
        public WhisperComposer(int virtualId, string text, int emotion, int colour)
            : base(ServerPacketHeader.WhisperMessageComposer)
        {
            WriteInteger(virtualId);
            WriteString(text);
            WriteInteger(emotion);
            WriteInteger(colour);

            WriteInteger(0);
            WriteInteger(-1);
        }
    }
}