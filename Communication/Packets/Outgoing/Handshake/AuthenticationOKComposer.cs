namespace Plus.Communication.Packets.Outgoing.Handshake
{
    public class AuthenticationOkComposer : ServerPacket
    {
        public AuthenticationOkComposer()
            : base(ServerPacketHeader.AuthenticationOkMessageComposer)
        {
        }
    }
}