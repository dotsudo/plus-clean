namespace Plus.Communication.Packets.Outgoing.Users
{
    using System.Linq;
    using HabboHotel.Users;

    internal class HabboUserBadgesComposer : ServerPacket
    {
        public HabboUserBadgesComposer(Habbo habbo)
            : base(ServerPacketHeader.HabboUserBadgesMessageComposer)
        {
            WriteInteger(habbo.Id);
            WriteInteger(habbo.GetBadgeComponent().EquippedCount);

            foreach (var badge in habbo.GetBadgeComponent().GetBadges().ToList())
            {
                if (badge.Slot <= 0)
                {
                    continue;
                }

                WriteInteger(badge.Slot);
                WriteString(badge.Code);
            }
        }
    }
}