namespace Plus.Communication.Packets.Incoming.Users
{
    using HabboHotel.GameClients;
    using HabboHotel.Quests;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Avatar;
    using Outgoing.Users;

    internal class RespectUserEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
            {
                return;
            }

            if (!session.GetHabbo().InRoom || session.GetHabbo().GetStats().DailyRespectPoints <= 0)
            {
                return;
            }

            Room room = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out room))
            {
                return;
            }

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(packet.PopInt());
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo().Id == session.GetHabbo().Id || user.IsBot)
            {
                return;
            }

            var thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (thisUser == null)
            {
                return;
            }

            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.SocialRespect);
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_RespectGiven", 1);
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(user.GetClient(), "ACH_RespectEarned", 1);

            session.GetHabbo().GetStats().DailyRespectPoints -= 1;
            session.GetHabbo().GetStats().RespectGiven += 1;
            user.GetClient().GetHabbo().GetStats().Respect += 1;

            if (room.RespectNotificationsEnabled)
            {
                room.SendPacket(new RespectNotificationComposer(user.GetClient().GetHabbo().Id, user.GetClient().GetHabbo().GetStats().Respect));
            }
            room.SendPacket(new ActionComposer(thisUser.VirtualId, 7));
        }
    }
}