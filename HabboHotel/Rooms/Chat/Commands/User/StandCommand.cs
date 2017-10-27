﻿namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    using GameClients;

    internal class StandCommand : IChatCommand
    {
        public string PermissionRequired => "command_stand";

        public string Parameters => "";

        public string Description => "Allows you to stand up if not stood already.";

        public void Execute(GameClient session, Room room, string[] Params)
        {
            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Username);
            if (user == null)
            {
                return;
            }

            if (user.isSitting)
            {
                user.Statusses.Remove("sit");
                user.Z += 0.35;
                user.isSitting = false;
                user.UpdateNeeded = true;
            }
            else if (user.isLying)
            {
                user.Statusses.Remove("lay");
                user.Z += 0.35;
                user.isLying = false;
                user.UpdateNeeded = true;
            }
        }
    }
}