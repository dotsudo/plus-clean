namespace Plus.Communication.Packets.Incoming.Groups
{
    using System;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using HabboHotel.Items;
    using Outgoing.Groups;
    using Outgoing.Rooms.Engine;

    internal class UpdateGroupColoursEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var groupId = packet.PopInt();
            var colour1 = packet.PopInt();
            var colour2 = packet.PopInt();

            Group group;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out group))
            {
                return;
            }

            if (group.CreatorId != session.GetHabbo().Id)
            {
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `groups` SET `colour1` = @colour1, `colour2` = @colour2 WHERE `id` = @groupId LIMIT 1");
                dbClient.AddParameter("colour1", colour1);
                dbClient.AddParameter("colour2", colour2);
                dbClient.AddParameter("groupId", group.Id);
                dbClient.RunQuery();
            }

            group.Colour1 = colour1;
            group.Colour2 = colour2;

            session.SendPacket(new GroupInfoComposer(group, session));

            if (session.GetHabbo().CurrentRoom == null)
            {
                return;
            }

            foreach (var item in session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetFloor.ToList())
            {
                if (item?.GetBaseItem() == null)
                {
                    continue;
                }

                if (item.GetBaseItem().InteractionType != InteractionType.GuildItem && item.GetBaseItem().InteractionType != InteractionType.GuildGate ||
                    item.GetBaseItem().InteractionType != InteractionType.GuildForum)
                {
                    continue;
                }

                session.GetHabbo().CurrentRoom.SendPacket(new ObjectUpdateComposer(item, Convert.ToInt32(item.UserId)));
            }
        }
    }
}