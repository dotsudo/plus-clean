namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Permissions;
    using Outgoing.Rooms.Settings;

    internal class AssignRightsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
            {
                return;
            }

            var userId = packet.PopInt();

            Room room;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out room))
            {
                return;
            }

            if (!room.CheckRights(session, true))
            {
                return;
            }

            if (room.UsersWithRights.Contains(userId))
            {
                session.SendNotification(PlusEnvironment.GetLanguageManager().TryGetValue("room.rights.user.has_rights"));
                return;
            }

            room.UsersWithRights.Add(userId);

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("INSERT INTO `room_rights` (`room_id`,`user_id`) VALUES ('" + room.RoomId + "','" + userId + "')");
            }

            var roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(userId);
            if (roomUser != null && !roomUser.IsBot)
            {
                roomUser.SetStatus("flatctrl 1");
                roomUser.UpdateNeeded = true;
                if (roomUser.GetClient() != null)
                {
                    roomUser.GetClient().SendPacket(new YouAreControllerComposer(1));
                }

                session.SendPacket(new FlatControllerAddedComposer(room.RoomId, roomUser.GetClient().GetHabbo().Id, roomUser.GetClient().GetHabbo().Username));
            }
            else
            {
                var user = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(userId);
                if (user != null)
                {
                    session.SendPacket(new FlatControllerAddedComposer(room.RoomId, user.Id, user.Username));
                }
            }
        }
    }
}