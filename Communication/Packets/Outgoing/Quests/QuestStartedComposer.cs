namespace Plus.Communication.Packets.Outgoing.Quests
{
    using HabboHotel.GameClients;
    using HabboHotel.Quests;

    internal class QuestStartedComposer : ServerPacket
    {
        public QuestStartedComposer(GameClient session, Quest quest)
            : base(ServerPacketHeader.QuestStartedMessageComposer)
        {
            SerializeQuest(this, session, quest, quest.Category);
        }

        private void SerializeQuest(ServerPacket message, GameClient session, Quest quest, string category)
        {
            if (message == null || session == null)
            {
                return;
            }

            var amountInCat = PlusEnvironment.GetGame().GetQuestManager().GetAmountOfQuestsInCategory(category);
            var number = quest?.Number - 1 ?? amountInCat;
            var userProgress = quest == null ? 0 : session.GetHabbo().GetQuestProgress(quest.Id);

            if (quest != null && quest.IsCompleted(userProgress))
            {
                number++;
            }

            message.WriteString(category);
            message.WriteInteger(quest == null ? 0 : (quest.Category.Contains("xmas2012") ? 0 : number)); // Quest progress in this cat
            message.WriteInteger(quest == null ? 0 : quest.Category.Contains("xmas2012") ? 0 : amountInCat); // Total quests in this cat
            message.WriteInteger(quest?.RewardType ?? 3); // Reward type (1 = Snowflakes, 2 = Love hearts, 3 = Pixels, 4 = Seashells, everything else is pixels
            message.WriteInteger(quest?.Id ?? 0); // Quest id
            message.WriteBoolean(quest != null && session.GetHabbo().GetStats().QuestID == quest.Id); // Quest started
            message.WriteString(quest == null ? string.Empty : quest.ActionName);
            message.WriteString(quest == null ? string.Empty : quest.DataBit);
            message.WriteInteger(quest?.Reward ?? 0);
            message.WriteString(quest == null ? string.Empty : quest.Name);
            message.WriteInteger(userProgress); // Current progress
            message.WriteInteger(quest?.GoalData ?? 0); // Target progress
            message.WriteInteger(quest?.TimeUnlock ?? 0); // "Next quest available countdown" in seconds
            message.WriteString("");
            message.WriteString("");
            message.WriteBoolean(true);
        }
    }
}