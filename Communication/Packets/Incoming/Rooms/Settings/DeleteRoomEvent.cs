namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using HabboHotel.Rooms;

    internal class DeleteRoomEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || session.GetHabbo().UsersRooms == null)
            {
                return;
            }

            var roomId = packet.PopInt();
            if (roomId == 0)
            {
                return;
            }

            Room room;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(roomId, out room))
            {
                return;
            }

            var data = room.RoomData;
            if (data == null)
            {
                return;
            }

            if (room.OwnerId != session.GetHabbo().Id && !session.GetHabbo().GetPermissions().HasRight("room_delete_any"))
            {
                return;
            }

            var itemsToRemove = new List<Item>();
            foreach (var item in room.GetRoomItemHandler().GetWallAndFloor.ToList())
            {
                if (item == null)
                {
                    continue;
                }

                if (item.GetBaseItem().InteractionType == InteractionType.Moodlight)
                {
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("DELETE FROM `room_items_moodlight` WHERE `item_id` = @itemId LIMIT 1");
                        dbClient.AddParameter("itemId", item.Id);
                        dbClient.RunQuery();
                    }
                }

                itemsToRemove.Add(item);
            }

            foreach (var item in itemsToRemove)
            {
                var targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(item.UserId);
                if (targetClient != null && targetClient.GetHabbo() != null) //Again, do we have an active client?
                {
                    room.GetRoomItemHandler().RemoveFurniture(targetClient, item.Id);
                    targetClient.GetHabbo().GetInventoryComponent().AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, item.LimitedNo, item.LimitedTot);
                    targetClient.GetHabbo().GetInventoryComponent().UpdateItems(false);
                }
                else //No, query time.
                {
                    room.GetRoomItemHandler().RemoveFurniture(null, item.Id);
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = @itemId LIMIT 1");
                        dbClient.AddParameter("itemId", item.Id);
                        dbClient.RunQuery();
                    }
                }
            }

            PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(room, true);

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM `user_roomvisits` WHERE `room_id` = '" + roomId + "'");
                dbClient.RunQuery("DELETE FROM `rooms` WHERE `id` = '" + roomId + "' LIMIT 1");
                dbClient.RunQuery("DELETE FROM `user_favorites` WHERE `room_id` = '" + roomId + "'");
                dbClient.RunQuery("DELETE FROM `items` WHERE `room_id` = '" + roomId + "'");
                dbClient.RunQuery("DELETE FROM `room_rights` WHERE `room_id` = '" + roomId + "'");
                dbClient.RunQuery("UPDATE `users` SET `home_room` = '0' WHERE `home_room` = '" + roomId + "'");
            }

            var removedRoom = (from p in session.GetHabbo().UsersRooms where p.Id == roomId select p).SingleOrDefault();
            if (removedRoom != null)
            {
                session.GetHabbo().UsersRooms.Remove(removedRoom);
            }
        }
    }
}