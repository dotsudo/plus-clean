namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;

    internal class ModerationMuteEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_mute"))
            {
                return;
            }

            var userId = packet.PopInt();

            packet.PopString();

            double length = packet.PopInt() * 60;

            packet.PopString();
            packet.PopString();

            var habbo = PlusEnvironment.GetHabboById(userId);

            if (habbo == null)
            {
                session.SendWhisper("An error occoured whilst finding that user in the database.");
                return;
            }

            if (habbo.GetPermissions().HasRight("mod_mute") && !session.GetHabbo().GetPermissions().HasRight("mod_mute_any"))
            {
                session.SendWhisper("Oops, you cannot mute that user.");
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `users` SET `time_muted` = '" + length + "' WHERE `id` = '" + habbo.Id + "' LIMIT 1");
            }

            if (habbo.GetClient() == null)
            {
                return;
            }

            habbo.TimeMuted = length;
            habbo.GetClient().SendNotification("You have been muted by a moderator for " + length + " seconds!");
        }
    }
}