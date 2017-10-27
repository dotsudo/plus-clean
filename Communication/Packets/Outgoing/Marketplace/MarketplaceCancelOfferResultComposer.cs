namespace Plus.Communication.Packets.Outgoing.Marketplace
{
    internal class MarketplaceCancelOfferResultComposer : ServerPacket
    {
        public MarketplaceCancelOfferResultComposer(int offerId, bool success)
            : base(ServerPacketHeader.MarketplaceCancelOfferResultMessageComposer)
        {
            WriteInteger(offerId);
            WriteBoolean(success);
        }
    }
}