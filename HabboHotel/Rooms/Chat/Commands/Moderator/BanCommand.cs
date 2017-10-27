﻿namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    using System;
    using GameClients;
    using Moderation;

    internal class BanCommand : IChatCommand
    {
        public string PermissionRequired => "command_ban";

        public string Parameters => "%username% %length% %reason% ";

        public string Description => "Remove a toxic player from the hotel for a fixed amount of time.";

        public void Execute(GameClient session, Room room, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Please enter the username of the user you'd like to IP ban & account ban.");
                return;
            }

            var habbo = PlusEnvironment.GetHabboByUsername(Params[1]);
            if (habbo == null)
            {
                session.SendWhisper("An error occoured whilst finding that user in the database.");
                return;
            }

            if (habbo.GetPermissions().HasRight("mod_soft_ban") && !session.GetHabbo().GetPermissions().HasRight("mod_ban_any"))
            {
                session.SendWhisper("Oops, you cannot ban that user.");
                return;
            }

            double expire = 0;
            var hours = Params[2];
            if (string.IsNullOrEmpty(hours) || hours == "perm")
            {
                expire = PlusEnvironment.GetUnixTimestamp() + 78892200;
            }
            else
            {
                expire = PlusEnvironment.GetUnixTimestamp() + Convert.ToDouble(hours) * 3600;
            }
            string reason = null;
            if (Params.Length >= 4)
            {
                reason = CommandManager.MergeParams(Params, 3);
            }
            else
            {
                reason = "No reason specified.";
            }
            var username = habbo.Username;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_info` SET `bans` = `bans` + '1' WHERE `user_id` = '" + habbo.Id + "' LIMIT 1");
            }
            PlusEnvironment.GetGame()
                .GetModerationManager()
                .BanUser(session.GetHabbo().Username, ModerationBanType.Username, habbo.Username, reason, expire);
            var targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username);
            if (targetClient != null)
            {
                targetClient.Disconnect();
            }
            session.SendWhisper("Success, you have account banned the user '" +
                                username +
                                "' for " +
                                hours +
                                " hour(s) with the reason '" +
                                reason +
                                "'!");
        }
    }
}