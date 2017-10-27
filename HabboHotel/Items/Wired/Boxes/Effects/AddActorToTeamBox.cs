namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    using System;
    using System.Collections.Concurrent;
    using Communication.Packets.Incoming;
    using Rooms;
    using Rooms.Games.Teams;
    using Users;

    internal class AddActorToTeamBox : IWiredItem
    {
        public AddActorToTeamBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.EffectAddActorToTeam;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket packet)
        {
            var unknown = packet.PopInt();
            var team = packet.PopInt();
            StringData = team.ToString();
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

            var toJoin = int.Parse(StringData) == 1
                ? TEAM.RED
                : int.Parse(StringData) == 2
                    ? TEAM.GREEN
                    : int.Parse(StringData) == 3
                        ? TEAM.BLUE
                        : int.Parse(StringData) == 4
                            ? TEAM.YELLOW
                            : TEAM.NONE;
            var team = Instance.GetTeamManagerForFreeze();
            if (team != null)
            {
                if (team.CanEnterOnTeam(toJoin))
                {
                    if (user.Team != TEAM.NONE)
                    {
                        team.OnUserLeave(user);
                    }
                    user.Team = toJoin;
                    team.AddUser(user);
                    if (user.GetClient().GetHabbo().Effects().CurrentEffect != Convert.ToInt32(toJoin + 39))
                    {
                        user.GetClient().GetHabbo().Effects().ApplyEffect(Convert.ToInt32(toJoin + 39));
                    }
                }
            }
            return true;
        }
    }
}