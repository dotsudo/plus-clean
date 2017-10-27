namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using HabboHotel.Rooms;
    using Outgoing.Inventory.Furni;
    using Outgoing.Inventory.Purse;

    internal class CreditFurniRedeemEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            Room room;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out room))
            {
                return;
            }

            if (!room.CheckRights(session, true))
            {
                return;
            }

            if (PlusEnvironment.GetSettingsManager().TryGetValue("room.item.exchangeables.enabled") != "1")
            {
                session.SendNotification("The hotel managers have temporarilly disabled exchanging!");
                return;
            }

            var exchange = room.GetRoomItemHandler().GetItem(packet.PopInt());
            if (exchange == null)
            {
                return;
            }

            if (exchange.Data.InteractionType != InteractionType.Exchange)
            {
                return;
            }

            var value = exchange.Data.BehaviourData;

            if (value > 0)
            {
                session.GetHabbo().Credits += value;
                session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM `items` WHERE `id` = @exchangeId LIMIT 1");
                dbClient.AddParameter("exchangeId", exchange.Id);
                dbClient.RunQuery();
            }

            session.SendPacket(new FurniListUpdateComposer());
            room.GetRoomItemHandler().RemoveFurniture(null, exchange.Id, false);
            session.GetHabbo().GetInventoryComponent().RemoveItem(exchange.Id);
        }
    }
}