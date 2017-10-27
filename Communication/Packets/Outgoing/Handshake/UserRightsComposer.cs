namespace Plus.Communication.Packets.Outgoing.Handshake
{
    public class UserRightsComposer : ServerPacket
    {
        public UserRightsComposer(int rank)
            : base(ServerPacketHeader.UserRightsMessageComposer)
        {
            WriteInteger(2); //Club level
            WriteInteger(rank);
            WriteBoolean(false); //Is an ambassador
        }
    }
}