﻿namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Permissions;
    using Outgoing.Rooms.Settings;

    internal class RemoveRightsEvent : IPacketEvent
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

            var amount = packet.PopInt();
            for (var i = 0; i < amount && i <= 100; i++)
            {
                var userId = packet.PopInt();
                
                if (userId <= 0 || !room.UsersWithRights.Contains(userId))
                {
                    continue;
                }

                var user = room.GetRoomUserManager().GetRoomUserByHabbo(userId);
                if (user != null && !user.IsBot)
                {
                    user.RemoveStatus("flatctrl 1");
                    user.UpdateNeeded = true;

                    user.GetClient().SendPacket(new YouAreControllerComposer(0));
                }

                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("DELETE FROM `room_rights` WHERE `user_id` = @uid AND `room_id` = @rid LIMIT 1");
                    dbClient.AddParameter("uid", userId);
                    dbClient.AddParameter("rid", room.Id);
                    dbClient.RunQuery();
                }

                if (room.UsersWithRights.Contains(userId))
                {
                    room.UsersWithRights.Remove(userId);
                }

                session.SendPacket(new FlatControllerRemovedComposer(room, userId));
            }
        }
    }
}