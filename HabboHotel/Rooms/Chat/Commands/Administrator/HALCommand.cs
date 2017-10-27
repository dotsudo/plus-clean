﻿namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator
{
    using Communication.Packets.Outgoing.Rooms.Notifications;
    using GameClients;

    internal class HALCommand : IChatCommand
    {
        public string PermissionRequired => "command_hal";

        public string Parameters => "%message%";

        public string Description => "Send a message to the entire hotel, with a link.";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 2)
            {
                Session.SendWhisper("Please enter a message and a URL to send..");
                return;
            }

            var URL = Params[1];
            var Message = CommandManager.MergeParams(Params, 2);
            PlusEnvironment.GetGame()
                .GetClientManager()
                .SendPacket(new RoomNotificationComposer("Habboon Hotel Alert!",
                    Message + "\r\n" + "- " + Session.GetHabbo().Username, "", URL, URL));
        }
    }
}