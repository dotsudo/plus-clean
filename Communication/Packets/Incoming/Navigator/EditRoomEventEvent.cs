namespace Plus.Communication.Packets.Incoming.Navigator
{
    using System;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Engine;

    internal class EditRoomEventEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var roomId = packet.PopInt();
            var name = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(packet.PopString());
            var desc = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(packet.PopString());

            var data = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomId);
            if (data == null)
            {
                return;
            }

            if (data.OwnerId != session.GetHabbo().Id)
            {
                return; //HAX
            }

            if (data.Promotion == null)
            {
                session.SendNotification("Oops, it looks like there isn't a room promotion in this room?");
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `room_promotions` SET `title` = @title, `description` = @desc WHERE `room_id` = " + roomId + " LIMIT 1");
                dbClient.AddParameter("title", name);
                dbClient.AddParameter("desc", desc);
                dbClient.RunQuery();
            }

            Room room;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Convert.ToInt32(roomId), out room))
            {
                return;
            }

            data.Promotion.Name = name;
            data.Promotion.Description = desc;
            room.SendPacket(new RoomEventComposer(data, data.Promotion));
        }
    }
}