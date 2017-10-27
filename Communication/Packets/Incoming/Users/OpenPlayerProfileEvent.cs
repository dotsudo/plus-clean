namespace Plus.Communication.Packets.Incoming.Users
{
    using HabboHotel.GameClients;
    using Outgoing.Users;

    internal class OpenPlayerProfileEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var userId = packet.PopInt();
            var isMe = packet.PopBoolean();

            var targetData = PlusEnvironment.GetHabboById(userId);
            if (targetData == null)
            {
                session.SendNotification("An error occured whilst finding that user's profile.");
                return;
            }

            var groups = PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(targetData.Id);

            var friendCount = 0;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT COUNT(0) FROM `messenger_friendships` WHERE (`user_one_id` = @userid OR `user_two_id` = @userid)");
                dbClient.AddParameter("userid", userId);
                friendCount = dbClient.GetInteger();
            }

            session.SendPacket(new ProfileInformationComposer(targetData, session, groups, friendCount));
        }
    }
}