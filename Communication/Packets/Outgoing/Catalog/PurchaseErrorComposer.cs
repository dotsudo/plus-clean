namespace Plus.Communication.Packets.Outgoing.Catalog
{
    internal class PurchaseErrorComposer : ServerPacket
    {
        public PurchaseErrorComposer(int errorCode)
            : base(ServerPacketHeader.PurchaseErrorMessageComposer)
        {
            WriteInteger(errorCode);
        }
    }
}