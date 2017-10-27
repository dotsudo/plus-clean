﻿namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    using GameClients;
    using Moderation;

    internal class MipCommand : IChatCommand
    {
        public string PermissionRequired => "command_mip";

        public string Parameters => "%username%";

        public string Description => "Machine ban, IP ban and account ban another user.";

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

            if (habbo.GetPermissions().HasRight("mod_tool") && !session.GetHabbo().GetPermissions().HasRight("mod_ban_any"))
            {
                session.SendWhisper("Oops, you cannot ban that user.");
                return;
            }

            var ipAddress = string.Empty;
            var expire = PlusEnvironment.GetUnixTimestamp() + 78892200;
            var username = habbo.Username;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_info` SET `bans` = `bans` + '1' WHERE `user_id` = '" + habbo.Id + "' LIMIT 1");
                dbClient.SetQuery("SELECT `ip_last` FROM `users` WHERE `id` = '" + habbo.Id + "' LIMIT 1");
                ipAddress = dbClient.GetString();
            }
            string reason = null;
            if (Params.Length >= 3)
            {
                reason = CommandManager.MergeParams(Params, 2);
            }
            else
            {
                reason = "No reason specified.";
            }
            if (!string.IsNullOrEmpty(ipAddress))
            {
                PlusEnvironment.GetGame().GetModerationManager().BanUser(session.GetHabbo().Username, ModerationBanType.Ip,
                    ipAddress, reason, expire);
            }
            PlusEnvironment.GetGame()
                .GetModerationManager()
                .BanUser(session.GetHabbo().Username, ModerationBanType.Username, habbo.Username, reason, expire);
            if (!string.IsNullOrEmpty(habbo.MachineId))
            {
                PlusEnvironment.GetGame()
                    .GetModerationManager()
                    .BanUser(session.GetHabbo().Username, ModerationBanType.Machine, habbo.MachineId, reason, expire);
            }
            var targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username);
            if (targetClient != null)
            {
                targetClient.Disconnect();
            }
            session.SendWhisper("Success, you have machine, IP and account banned the user '" + username + "' for '" + reason +
                                "'!");
        }
    }
}