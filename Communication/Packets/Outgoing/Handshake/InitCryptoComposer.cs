namespace Plus.Communication.Packets.Outgoing.Handshake
{
    public class InitCryptoComposer : ServerPacket
    {
        public InitCryptoComposer(string prime, string generator)
            : base(ServerPacketHeader.InitCryptoMessageComposer)
        {
            WriteString(prime);
            WriteString(generator);
        }
    }
}