namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using HabboHotel.Users.Messenger;
    using Outgoing.Catalog;
    using Outgoing.Rooms.Engine;

    internal class PurchaseRoomPromotionEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
            {
                return;
            }

            packet.PopInt();
            packet.PopInt();

            var roomId = packet.PopInt();
            var name = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(packet.PopString());

            packet.PopBoolean();

            var desc = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(packet.PopString());
            var categoryId = packet.PopInt();

            var data = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomId);

            if (data?.OwnerId != session.GetHabbo().Id)
            {
                return;
            }

            if (data.Promotion == null)
            {
                data.Promotion = new RoomPromotion(name, desc, categoryId);
            }
            else
            {
                data.Promotion.Name = name;
                data.Promotion.Description = desc;
                data.Promotion.TimestampExpires += 7200;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "REPLACE INTO `room_promotions` (`room_id`,`title`,`description`,`timestamp_start`,`timestamp_expire`,`category_id`) VALUES (@room_id, @title, @description, @start, @expires, @CategoryId)");
                dbClient.AddParameter("room_id", roomId);
                dbClient.AddParameter("title", name);
                dbClient.AddParameter("description", desc);
                dbClient.AddParameter("start", data.Promotion.TimestampStarted);
                dbClient.AddParameter("expires", data.Promotion.TimestampExpires);
                dbClient.AddParameter("CategoryId", categoryId);
                dbClient.RunQuery();
            }

            if (!session.GetHabbo().GetBadgeComponent().HasBadge("RADZZ"))
            {
                session.GetHabbo().GetBadgeComponent().GiveBadge("RADZZ", true, session);
            }

            session.SendPacket(new PurchaseOkComposer());
            if (session.GetHabbo().InRoom && session.GetHabbo().CurrentRoomId == roomId)
            {
                session.GetHabbo().CurrentRoom.SendPacket(new RoomEventComposer(data, data.Promotion));
            }

            session.GetHabbo().GetMessenger().BroadcastAchievement(session.GetHabbo().Id, MessengerEventTypes.EventStarted, name);
        }
    }
}