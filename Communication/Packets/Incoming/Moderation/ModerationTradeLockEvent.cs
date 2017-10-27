namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;

    internal class ModerationTradeLockEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_trade_lock"))
            {
                return;
            }

            var userId = packet.PopInt();
            var message = packet.PopString();
            double days = packet.PopInt() / 1440;
            packet.PopString();
            packet.PopString();

            var length = PlusEnvironment.GetUnixTimestamp() + days * 86400;

            var habbo = PlusEnvironment.GetHabboById(userId);
            if (habbo == null)
            {
                session.SendWhisper("An error occoured whilst finding that user in the database.");
                return;
            }

            if (habbo.GetPermissions().HasRight("mod_trade_lock") && !session.GetHabbo().GetPermissions().HasRight("mod_trade_lock_any"))
            {
                session.SendWhisper("Oops, you cannot trade lock another user ranked 5 or higher.");
                return;
            }

            if (days < 1)
            {
                days = 1;
            }

            if (days > 365)
            {
                days = 365;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_info` SET `trading_locked` = '" + length + "', `trading_locks_count` = `trading_locks_count` + '1' WHERE `user_id` = '" + habbo.Id +
                                  "' LIMIT 1");
            }

            if (habbo.GetClient() == null)
            {
                return;
            }

            habbo.TradingLockExpiry = length;
            habbo.GetClient().SendNotification("You have been trade banned for " + days + " day(s)!\r\rReason:\r\r" + message);
        }
    }
}