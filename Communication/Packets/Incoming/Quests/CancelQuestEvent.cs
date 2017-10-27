namespace Plus.Communication.Packets.Incoming.Quests
{
    using HabboHotel.GameClients;
    using Outgoing.Quests;

    internal class CancelQuestEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var quest = PlusEnvironment.GetGame().GetQuestManager().GetQuest(session.GetHabbo().GetStats().QuestID);
            if (quest == null)
            {
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM `user_quests` WHERE `user_id` = '" + session.GetHabbo().Id + "' AND `quest_id` = '" + quest.Id + "';" +
                                  "UPDATE `user_stats` SET `quest_id` = '0' WHERE `id` = '" + session.GetHabbo().Id + "' LIMIT 1");
            }

            session.GetHabbo().GetStats().QuestID = 0;
            session.SendPacket(new QuestAbortedComposer());

            PlusEnvironment.GetGame().GetQuestManager().GetList(session, null);
        }
    }
}