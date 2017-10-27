namespace Plus.Communication.Packets.Outgoing.Quests
{
    using HabboHotel.GameClients;
    using HabboHotel.Quests;

    internal class QuestCompletedComposer : ServerPacket
    {
        public QuestCompletedComposer(GameClient session, Quest quest)
            : base(ServerPacketHeader.QuestCompletedMessageComposer)
        {
            var amountInCat = PlusEnvironment.GetGame().GetQuestManager().GetAmountOfQuestsInCategory(quest.Category);
            var number = quest.Number;
            var userProgress = session.GetHabbo().GetQuestProgress(quest.Id);

            WriteString(quest.Category);
            WriteInteger(number); // Quest progress in this cat
            WriteInteger(quest.Name.Contains("xmas2012") ? 1 : amountInCat); // Total quests in this cat
            WriteInteger(quest?.RewardType ?? 3); // Reward type (1 = Snowflakes, 2 = Love hearts, 3 = Pixels, 4 = Seashells, everything else is pixels
            WriteInteger(quest?.Id ?? 0); // Quest id
            WriteBoolean(session.GetHabbo().GetStats().QuestID == quest.Id); // Quest started
            WriteString(quest.ActionName);
            WriteString(quest.DataBit);
            WriteInteger(quest?.Reward ?? 0);
            WriteString(quest.Name);
            WriteInteger(userProgress); // Current progress
            WriteInteger(quest?.GoalData ?? 0); // Target progress
            WriteInteger(quest?.TimeUnlock ?? 0); // "Next quest available countdown" in seconds
            WriteString("");
            WriteString("");
            WriteBoolean(true); // ?
            WriteBoolean(true); // Activate next quest..
        }
    }
}