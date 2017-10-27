namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.Catalog;
    using HabboHotel.GameClients;
    using Outgoing.Catalog;

    internal class GetCatalogOfferEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var offerId = packet.PopInt();
            if (!PlusEnvironment.GetGame().GetCatalog().ItemOffers.ContainsKey(offerId))
            {
                return;
            }

            var pageId = PlusEnvironment.GetGame().GetCatalog().ItemOffers[offerId];

            CatalogPage page;
            if (!PlusEnvironment.GetGame().GetCatalog().TryGetPage(pageId, out page))
            {
                return;
            }

            if (!page.Enabled || !page.Visible || page.MinimumRank > session.GetHabbo().Rank || page.MinimumVip > session.GetHabbo().VipRank && session.GetHabbo().Rank == 1)
            {
                return;
            }

            if (!page.ItemOffers.ContainsKey(offerId))
            {
                return;
            }

            var item = page.ItemOffers[offerId];

            if (item != null)
            {
                session.SendPacket(new CatalogOfferComposer(item));
            }
        }
    }
}