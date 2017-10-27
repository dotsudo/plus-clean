﻿namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Engine;

    internal class SetTonerEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            Room room;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out room))
            {
                return;
            }

            if (!room.CheckRights(session, true))
            {
                return;
            }

            if (room.TonerData == null)
            {
                return;
            }

            var item = room.GetRoomItemHandler().GetItem(room.TonerData.ItemId);

            if (item == null || item.GetBaseItem().InteractionType != InteractionType.Toner)
            {
                return;
            }

            var id = packet.PopInt();
            var int1 = packet.PopInt();
            var int2 = packet.PopInt();
            var int3 = packet.PopInt();

            if (int1 > 255 || int2 > 255 || int3 > 255)
            {
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `room_items_toner` SET `enabled` = '1', `data1` = @data1, `data2` = @data2, `data3` = @data3 WHERE `id` = @itemId LIMIT 1");
                dbClient.AddParameter("itemId", item.Id);
                dbClient.AddParameter("data1", int1);
                dbClient.AddParameter("data3", int3);
                dbClient.AddParameter("data2", int2);
                dbClient.RunQuery();
            }

            room.TonerData.Hue = int1;
            room.TonerData.Saturation = int2;
            room.TonerData.Lightness = int3;
            room.TonerData.Enabled = 1;

            room.SendPacket(new ObjectUpdateComposer(item, room.OwnerId));
            item.UpdateState();
        }
    }
}