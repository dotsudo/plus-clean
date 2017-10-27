namespace Plus.Communication.Packets.Outgoing.Messenger
{
    internal class MessengerErrorComposer : ServerPacket
    {
        public MessengerErrorComposer(int errorCode1, int errorCode2)
            : base(ServerPacketHeader.MessengerErrorMessageComposer)
        {
            WriteInteger(errorCode1);
            WriteInteger(errorCode2);
        }
    }
}