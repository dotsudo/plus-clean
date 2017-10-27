namespace Plus.Communication.Packets.Outgoing.Inventory.Badges
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Users.Badges;

    internal class BadgesComposer : ServerPacket
    {
        public BadgesComposer(GameClient session)
            : base(ServerPacketHeader.BadgesMessageComposer)
        {
            var equippedBadges = new List<Badge>();

            WriteInteger(session.GetHabbo().GetBadgeComponent().Count);
            foreach (var badge in session.GetHabbo().GetBadgeComponent().GetBadges().ToList())
            {
                WriteInteger(1);
                WriteString(badge.Code);

                if (badge.Slot > 0)
                {
                    equippedBadges.Add(badge);
                }
            }

            WriteInteger(equippedBadges.Count);
            foreach (var badge in equippedBadges)
            {
                WriteInteger(badge.Slot);
                WriteString(badge.Code);
            }
        }
    }
}