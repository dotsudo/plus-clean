namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    using System.Linq;
    using GameClients;

    internal class GoBoomCommand : IChatCommand
    {
        public string PermissionRequired => "command_goboom";

        public string Parameters => "";

        public string Description => "Make the entire room go boom! (Applys effect 108)";

        public void Execute(GameClient session, Room room, string[] Params)
        {
            var users = room.GetRoomUserManager().GetRoomUsers();
            if (users.Count > 0)
            {
                foreach (var u in users.ToList())
                {
                    if (u == null)
                    {
                        continue;
                    }

                    u.ApplyEffect(108);
                }
            }
        }
    }
}