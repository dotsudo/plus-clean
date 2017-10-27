namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;

    internal class ModerationCautionEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_caution"))
            {
                return;
            }

            var userId = packet.PopInt();
            var message = packet.PopString();

            var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(userId);
            if (client?.GetHabbo() == null)
            {
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_info` SET `cautions` = `cautions` + '1' WHERE `user_id` = '" + client.GetHabbo().Id + "' LIMIT 1");
            }

            client.SendNotification(message);
        }
    }
}