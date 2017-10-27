namespace Plus.Communication.Packets.Outgoing.Marketplace
{
    internal class MarketplaceItemStatsComposer : ServerPacket
    {
        public MarketplaceItemStatsComposer(int itemId, int spriteId, int averagePrice)
            : base(ServerPacketHeader.MarketplaceItemStatsMessageComposer)
        {
            WriteInteger(averagePrice); //Avg price in last 7 days.
            WriteInteger(PlusEnvironment.GetGame().GetCatalog().GetMarketplace().OfferCountForSprite(spriteId));

            WriteInteger(0); //No idea.
            WriteInteger(0); //No idea.

            WriteInteger(itemId);
            WriteInteger(spriteId);
        }
    }
}