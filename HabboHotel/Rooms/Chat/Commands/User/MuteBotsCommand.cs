﻿namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    using GameClients;

    internal class MuteBotsCommand : IChatCommand
    {
        public string PermissionRequired => "command_mute_bots";

        public string Parameters => "";

        public string Description => "Ignore bot chat or enable it again.";

        public void Execute(GameClient session, Room room, string[] Params)
        {
            session.GetHabbo().AllowBotSpeech = !session.GetHabbo().AllowBotSpeech;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `users` SET `bots_muted` = '" +
                                  (session.GetHabbo().AllowBotSpeech ? 1 : 0) +
                                  "' WHERE `id` = '" +
                                  session.GetHabbo().Id +
                                  "' LIMIT 1");
            }
            if (session.GetHabbo().AllowBotSpeech)
            {
                session.SendWhisper("Change successful, you can no longer see speech from bots.");
            }
            else
            {
                session.SendWhisper("Change successful, you can now see speech from bots.");
            }
        }
    }
}