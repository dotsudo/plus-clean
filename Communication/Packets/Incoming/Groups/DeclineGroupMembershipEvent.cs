namespace Plus.Communication.Packets.Incoming.Groups
{
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using Outgoing.Groups;

    internal class DeclineGroupMembershipEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var groupId = packet.PopInt();
            var userId = packet.PopInt();

            Group group;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out group))
            {
                return;
            }

            if (session.GetHabbo().Id != group.CreatorId && !group.IsAdmin(session.GetHabbo().Id))
            {
                return;
            }

            if (!group.HasRequest(userId))
            {
                return;
            }

            group.HandleRequest(userId, false);
            session.SendPacket(new UnknownGroupComposer(group.Id, userId));
        }
    }
}