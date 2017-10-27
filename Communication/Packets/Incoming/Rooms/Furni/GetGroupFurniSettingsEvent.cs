﻿namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using HabboHotel.Items;
    using Outgoing.Groups;

    internal class GetGroupFurniSettingsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().InRoom)
            {
                return;
            }

            var itemId = packet.PopInt();
            var groupId = packet.PopInt();

            var item = session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetItem(itemId);
            if (item == null)
            {
                return;
            }

            if (item.Data.InteractionType != InteractionType.GuildGate)
            {
                return;
            }

            Group group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out group))
            {
                return;
            }

            session.SendPacket(new GroupFurniSettingsComposer(group, itemId, session.GetHabbo().Id));
            session.SendPacket(new GroupInfoComposer(group, session));
        }
    }
}