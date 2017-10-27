namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Action;

    internal class IgnoreUserEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            var username = packet.PopString();

            var player = PlusEnvironment.GetHabboByUsername(username);
            if (player == null || player.GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            if (session.GetHabbo().GetIgnores().TryGet(player.Id))
            {
                return;
            }

            if (!session.GetHabbo().GetIgnores().TryAdd(player.Id))
            {
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `user_ignores` (`user_id`,`ignore_id`) VALUES(@uid,@ignoreId);");
                dbClient.AddParameter("uid", session.GetHabbo().Id);
                dbClient.AddParameter("ignoreId", player.Id);
                dbClient.RunQuery();
            }

            session.SendPacket(new IgnoreStatusComposer(1, player.Username));

            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_SelfModIgnoreSeen", 1);
        }
    }
}