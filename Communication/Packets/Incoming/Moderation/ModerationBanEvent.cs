namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;
    using HabboHotel.Moderation;

    internal class ModerationBanEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_soft_ban"))
            {
                return;
            }

            var userId = packet.PopInt();
            var message = packet.PopString();
            var length = packet.PopInt() * 3600 + PlusEnvironment.GetUnixTimestamp();
            packet.PopString(); // unknown
            packet.PopString(); // unknown
            var ipBan = packet.PopBoolean();
            var machineBan = packet.PopBoolean();

            if (machineBan)
            {
                ipBan = false;
            }

            var habbo = PlusEnvironment.GetHabboById(userId);

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

            message = message ?? "No reason specified.";

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_info` SET `bans` = `bans` + '1' WHERE `user_id` = '" + habbo.Id + "' LIMIT 1");
            }

            if (ipBan == false && machineBan == false)
            {
                PlusEnvironment.GetGame().GetModerationManager().BanUser(session.GetHabbo().Username, ModerationBanType.Username, habbo.Username, message, length);
            }
            else if (ipBan)
            {
                PlusEnvironment.GetGame().GetModerationManager().BanUser(session.GetHabbo().Username, ModerationBanType.Ip, habbo.Username, message, length);
            }
            else if (machineBan)
            {
                PlusEnvironment.GetGame().GetModerationManager().BanUser(session.GetHabbo().Username, ModerationBanType.Ip, habbo.Username, message, length);
                PlusEnvironment.GetGame().GetModerationManager().BanUser(session.GetHabbo().Username, ModerationBanType.Username, habbo.Username, message, length);
                PlusEnvironment.GetGame().GetModerationManager().BanUser(session.GetHabbo().Username, ModerationBanType.Machine, habbo.Username, message, length);
            }

            var targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(habbo.Username);
            if (targetClient != null)
            {
                targetClient.Disconnect();
            }
        }
    }
}