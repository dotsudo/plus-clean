namespace Plus.Communication.Packets.Outgoing.Groups
{
    using System;
    using HabboHotel.GameClients;
    using HabboHotel.Groups;

    internal class GroupInfoComposer : ServerPacket
    {
        public GroupInfoComposer(Group group, GameClient session, bool newWindow = false)
            : base(ServerPacketHeader.GroupInfoMessageComposer)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(group.CreateTime);

            WriteInteger(group.Id);
            WriteBoolean(true);
            WriteInteger(group.GroupType == GroupType.OPEN ? 0 : group.GroupType == GroupType.LOCKED ? 1 : 2);
            WriteString(group.Name);
            WriteString(group.Description);
            WriteString(group.Badge);
            WriteInteger(group.RoomId);
            WriteString(PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(group.RoomId) == null
                ? "No room found.."
                : PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(group.RoomId).Name); // room name
            WriteInteger(group.CreatorId == session.GetHabbo().Id ? 3 : group.HasRequest(session.GetHabbo().Id) ? 2 : group.IsMember(session.GetHabbo().Id) ? 1 : 0);
            WriteInteger(group.MemberCount); // Members
            WriteBoolean(false); //?? CHANGED
            WriteString(origin.Day + "-" + origin.Month + "-" + origin.Year);
            WriteBoolean(group.CreatorId == session.GetHabbo().Id);
            WriteBoolean(group.IsAdmin(session.GetHabbo().Id)); // admin
            WriteString(PlusEnvironment.GetUsernameById(group.CreatorId));
            WriteBoolean(newWindow); // Show group info
            WriteBoolean(group.AdminOnlyDeco == 0); // Any user can place furni in home room
            WriteInteger(group.CreatorId == session.GetHabbo().Id
                ? group.RequestCount
                : group.IsAdmin(session.GetHabbo().Id)
                    ? group.RequestCount
                    : group.IsMember(session.GetHabbo().Id)
                        ? 0
                        : 0); // Pending users

            //base.WriteInteger(0);//what the fuck
            WriteBoolean(group != null ? group.ForumEnabled : true); //HabboTalk.
        }
    }
}