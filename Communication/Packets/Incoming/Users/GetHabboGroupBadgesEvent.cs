﻿namespace Plus.Communication.Packets.Incoming.Users
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using Outgoing.Users;

    internal class GetHabboGroupBadgesEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().InRoom)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            var badges = new Dictionary<int, string>();
            foreach (var user in room.GetRoomUserManager().GetRoomUsers().ToList())
            {
                if (user.IsBot || user.IsPet || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                {
                    continue;
                }

                if (user.GetClient().GetHabbo().GetStats().FavouriteGroupId == 0 || badges.ContainsKey(user.GetClient().GetHabbo().GetStats().FavouriteGroupId))
                {
                    continue;
                }

                Group group = null;
                if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(user.GetClient().GetHabbo().GetStats().FavouriteGroupId, out group))
                {
                    continue;
                }

                if (!badges.ContainsKey(group.Id))
                {
                    badges.Add(group.Id, group.Badge);
                }
            }

            if (session.GetHabbo().GetStats().FavouriteGroupId > 0)
            {
                Group group = null;
                if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(session.GetHabbo().GetStats().FavouriteGroupId, out group))
                {
                    if (!badges.ContainsKey(group.Id))
                    {
                        badges.Add(group.Id, group.Badge);
                    }
                }
            }

            room.SendPacket(new HabboGroupBadgesComposer(badges));
            session.SendPacket(new HabboGroupBadgesComposer(badges));
        }
    }
}