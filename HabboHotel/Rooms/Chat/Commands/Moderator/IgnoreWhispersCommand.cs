﻿namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    using GameClients;

    internal class IgnoreWhispersCommand : IChatCommand
    {
        public string PermissionRequired => "command_ignore_whispers";

        public string Parameters => "";

        public string Description => "Allows you to ignore all of the whispers in the room, except from your own.";

        public void Execute(GameClient session, Room room, string[] Params)
        {
            session.GetHabbo().IgnorePublicWhispers = !session.GetHabbo().IgnorePublicWhispers;
            session.SendWhisper("You're " + (session.GetHabbo().IgnorePublicWhispers ? "now" : "no longer") +
                                " ignoring public whispers!");
        }
    }
}