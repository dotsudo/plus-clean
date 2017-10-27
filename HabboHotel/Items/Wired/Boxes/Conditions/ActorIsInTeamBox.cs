namespace Plus.HabboHotel.Items.Wired.Boxes.Conditions
{
    using System.Collections.Concurrent;
    using Communication.Packets.Incoming;
    using Rooms;
    using Rooms.Games.Teams;
    using Users;

    internal class ActorIsInTeamBox : IWiredItem
    {
        public ActorIsInTeamBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.ConditionActorIsInTeamBox;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket packet)
        {
            var unknown = packet.PopInt();
            var unknown2 = packet.PopInt();
            StringData = unknown2.ToString();
        }

        public bool Execute(params object[] Params)
        {
            if (Params.Length == 0 || Instance == null || string.IsNullOrEmpty(StringData))
            {
                return false;
            }

            var player = (Habbo) Params[0];
            if (player == null)
            {
                return false;
            }

            var user = Instance.GetRoomUserManager().GetRoomUserByHabbo(player.Id);
            if (user == null)
            {
                return false;
            }
            if (int.Parse(StringData) == 1 && user.Team == TEAM.RED)
            {
                return true;
            }
            if (int.Parse(StringData) == 2 && user.Team == TEAM.GREEN)
            {
                return true;
            }
            if (int.Parse(StringData) == 3 && user.Team == TEAM.BLUE)
            {
                return true;
            }
            if (int.Parse(StringData) == 4 && user.Team == TEAM.YELLOW)
            {
                return true;
            }

            return false;
        }
    }
}