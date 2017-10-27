namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.GameClients;
    using Outgoing.BuildersClub;
    using Outgoing.Catalog;

    public class GetCatalogIndexEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            /*int Sub = 0;

            if (Session.GetHabbo().GetSubscriptionManager().HasSubscription)
            {
                Sub = Session.GetHabbo().GetSubscriptionManager().GetSubscription().SubscriptionId;
            }*/

            session.SendPacket(new CatalogIndexComposer(session, PlusEnvironment.GetGame().GetCatalog().GetPages())); //, Sub));
            session.SendPacket(new CatalogItemDiscountComposer());
            session.SendPacket(new BcBorrowedItemsComposer());
        }
    }
}