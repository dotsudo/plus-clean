namespace Plus.Communication.Packets.Incoming.Messenger
{
    using HabboHotel.GameClients;
    using HabboHotel.Quests;

    internal class RequestBuddyEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            if (session.GetHabbo().GetMessenger().RequestBuddy(packet.PopString()))
            {
                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.SocialFriend);
            }
        }
    }
}