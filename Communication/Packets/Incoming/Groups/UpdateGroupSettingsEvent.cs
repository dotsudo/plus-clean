namespace Plus.Communication.Packets.Incoming.Groups
{
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using HabboHotel.Rooms;
    using Outgoing.Groups;
    using Outgoing.Rooms.Permissions;

    internal class UpdateGroupSettingsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var groupId = packet.PopInt();

            Group group;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out group))
            {
                return;
            }

            if (group.CreatorId != session.GetHabbo().Id)
            {
                return;
            }

            var type = packet.PopInt();
            var furniOptions = packet.PopInt();

            switch (type)
            {
                default:
                    group.GroupType = GroupType.OPEN;
                    break;
                case 1:
                    group.GroupType = GroupType.LOCKED;
                    break;
                case 2:
                    group.GroupType = GroupType.PRIVATE;
                    break;
            }

            if (group.GroupType != GroupType.LOCKED)
            {
                if (group.GetRequests.Count > 0)
                {
                    foreach (var userId in group.GetRequests.ToList())
                    {
                        group.HandleRequest(userId, false);
                    }

                    group.ClearRequests();
                }
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `groups` SET `state` = @GroupState, `admindeco` = @AdminDeco WHERE `id` = @groupId LIMIT 1");
                dbClient.AddParameter("GroupState", (group.GroupType == GroupType.OPEN ? 0 : group.GroupType == GroupType.LOCKED ? 1 : 2).ToString());
                dbClient.AddParameter("AdminDeco", (furniOptions == 1 ? 1 : 0).ToString());
                dbClient.AddParameter("groupId", group.Id);
                dbClient.RunQuery();
            }

            group.AdminOnlyDeco = furniOptions;

            Room room;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(group.RoomId, out room))
            {
                return;
            }

            foreach (var user in room.GetRoomUserManager().GetRoomUsers().ToList())
            {
                if (room.OwnerId == user.UserId || group.IsAdmin(user.UserId) || !group.IsMember(user.UserId))
                {
                    continue;
                }

                if (furniOptions == 1)
                {
                    user.RemoveStatus("flatctrl 1");
                    user.UpdateNeeded = true;

                    user.GetClient().SendPacket(new YouAreControllerComposer(0));
                }
                else if (furniOptions == 0 && !user.Statusses.ContainsKey("flatctrl 1"))
                {
                    user.SetStatus("flatctrl 1");
                    user.UpdateNeeded = true;

                    user.GetClient().SendPacket(new YouAreControllerComposer(1));
                }
            }

            session.SendPacket(new GroupInfoComposer(group, session));
        }
    }
}