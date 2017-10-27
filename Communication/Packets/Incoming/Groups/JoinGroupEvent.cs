namespace Plus.Communication.Packets.Incoming.Groups
{
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using Outgoing.Catalog;
    using Outgoing.Groups;
    using Outgoing.Moderation;

    internal class JoinGroupEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
            {
                return;
            }

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(packet.PopInt(), out var group))
            {
                return;
            }

            if (group.IsMember(session.GetHabbo().Id) || group.IsAdmin(session.GetHabbo().Id) || group.HasRequest(session.GetHabbo().Id) && group.GroupType == GroupType.PRIVATE)
            {
                return;
            }

            var groups = PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(session.GetHabbo().Id);
            if (groups.Count >= 1500)
            {
                session.SendPacket(new BroadcastMessageAlertComposer("Oops, it appears that you've hit the group membership limit! You can only join upto 1,500 groups."));
                return;
            }

            group.AddMember(session.GetHabbo().Id);

            if (group.GroupType == GroupType.LOCKED)
            {
                var groupAdmins = (from client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList()
                    where client?.GetHabbo() != null && @group.IsAdmin(client.GetHabbo().Id)
                    select client).ToList();
                foreach (var client in groupAdmins)
                {
                    client.SendPacket(new GroupMembershipRequestedComposer(group.Id, session.GetHabbo(), 3));
                }

                session.SendPacket(new GroupInfoComposer(group, session));
            }
            else
            {
                session.SendPacket(new GroupFurniConfigComposer(PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(session.GetHabbo().Id)));
                session.SendPacket(new GroupInfoComposer(group, session));

                if (session.GetHabbo().CurrentRoom != null)
                {
                    session.GetHabbo().CurrentRoom.SendPacket(new RefreshFavouriteGroupComposer(session.GetHabbo().Id));
                }
                else
                {
                    session.SendPacket(new RefreshFavouriteGroupComposer(session.GetHabbo().Id));
                }
            }
        }
    }
}