﻿namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    using GameClients;

    internal class MoonwalkCommand : IChatCommand
    {
        public string PermissionRequired => "command_moonwalk";

        public string Parameters => "";

        public string Description => "Wear the shoes of Michael Jackson.";

        public void Execute(GameClient session, Room room, string[] Params)
        {
            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
            {
                return;
            }

            user.moonwalkEnabled = !user.moonwalkEnabled;
            if (user.moonwalkEnabled)
            {
                session.SendWhisper("Moonwalk enabled!");
            }
            else
            {
                session.SendWhisper("Moonwalk disabled!");
            }
        }
    }
}