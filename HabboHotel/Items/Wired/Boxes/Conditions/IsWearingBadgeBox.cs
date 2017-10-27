namespace Plus.HabboHotel.Items.Wired.Boxes.Conditions
{
    using System.Collections.Concurrent;
    using System.Linq;
    using Communication.Packets.Incoming;
    using Rooms;
    using Users;

    internal class IsWearingBadgeBox : IWiredItem
    {
        public IsWearingBadgeBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.ConditionIsWearingBadge;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket packet)
        {
            var unknown = packet.PopInt();
            var badgeCode = packet.PopString();
            StringData = badgeCode;
        }

        public bool Execute(params object[] Params)
        {
            if (Params.Length == 0)
            {
                return false;
            }
            if (string.IsNullOrEmpty(StringData))
            {
                return false;
            }

            var player = (Habbo) Params[0];
            if (player == null)
            {
                return false;
            }
            if (!player.GetBadgeComponent().GetBadges().Contains(player.GetBadgeComponent().GetBadge(StringData)))
            {
                return false;
            }

            foreach (var badge in player.GetBadgeComponent().GetBadges().ToList())
            {
                if (badge.Slot <= 0)
                {
                    continue;
                }

                if (badge.Code == StringData)
                {
                    return true;
                }
            }

            return false;
        }
    }
}