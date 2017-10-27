namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    using System;
    using Communication.Packets.Outgoing.Rooms.Chat;
    using GameClients;

    internal class PushCommand : IChatCommand
    {
        public string PermissionRequired => "command_push";

        public string Parameters => "%target%";

        public string Description => "Push another user.";

        public void Execute(GameClient session, Room room, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Please enter the username of the user you wish to push.");
                return;
            }

            if (!room.PushEnabled && !session.GetHabbo().GetPermissions().HasRight("room_override_custom_config"))
            {
                session.SendWhisper(
                    "Oops, it appears that the room owner has disabled the ability to use the push command in here.");
                return;
            }

            var targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            var targetUser = room.GetRoomUserManager().GetRoomUserByHabbo(targetClient.GetHabbo().Id);
            if (targetUser == null)
            {
                session.SendWhisper("An error occoured whilst finding that user, maybe they're not online or in this room.");
                return;
            }

            if (targetClient.GetHabbo().Username == session.GetHabbo().Username)
            {
                session.SendWhisper("Come on, surely you don't want to push yourself!");
                return;
            }

            if (targetUser.TeleportEnabled)
            {
                session.SendWhisper("Oops, you cannot push a user whilst they have their teleport mode enabled.");
                return;
            }

            var thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (thisUser == null)
            {
                return;
            }

            if (!(Math.Abs(targetUser.X - thisUser.X) >= 2 || Math.Abs(targetUser.Y - thisUser.Y) >= 2))
            {
                if (targetUser.SetX - 1 == room.GetGameMap().Model.DoorX)
                {
                    session.SendWhisper("Please don't push that user out of the room :(!");
                    return;
                }

                if (targetUser.RotBody == 4)
                {
                    targetUser.MoveTo(targetUser.X, targetUser.Y + 1);
                }
                if (thisUser.RotBody == 0)
                {
                    targetUser.MoveTo(targetUser.X, targetUser.Y - 1);
                }
                if (thisUser.RotBody == 6)
                {
                    targetUser.MoveTo(targetUser.X - 1, targetUser.Y);
                }
                if (thisUser.RotBody == 2)
                {
                    targetUser.MoveTo(targetUser.X + 1, targetUser.Y);
                }
                if (thisUser.RotBody == 3)
                {
                    targetUser.MoveTo(targetUser.X + 1, targetUser.Y);
                    targetUser.MoveTo(targetUser.X, targetUser.Y + 1);
                }
                if (thisUser.RotBody == 1)
                {
                    targetUser.MoveTo(targetUser.X + 1, targetUser.Y);
                    targetUser.MoveTo(targetUser.X, targetUser.Y - 1);
                }
                if (thisUser.RotBody == 7)
                {
                    targetUser.MoveTo(targetUser.X - 1, targetUser.Y);
                    targetUser.MoveTo(targetUser.X, targetUser.Y - 1);
                }
                if (thisUser.RotBody == 5)
                {
                    targetUser.MoveTo(targetUser.X - 1, targetUser.Y);
                    targetUser.MoveTo(targetUser.X, targetUser.Y + 1);
                }
                room.SendPacket(new ChatComposer(thisUser.VirtualId, "*pushes " + Params[1] + "*", 0, thisUser.LastBubble));
            }
            else
            {
                session.SendWhisper("Oops, " + Params[1] + " is not close enough!");
            }
        }
    }
}