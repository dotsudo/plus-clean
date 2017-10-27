namespace Plus.HabboHotel.Quests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Communication.Packets.Incoming;
    using Communication.Packets.Outgoing.Inventory.Purse;
    using Communication.Packets.Outgoing.Quests;
    using GameClients;
    using log4net;
    using Users.Messenger;

    public class QuestManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Quests.QuestManager");
        private readonly Dictionary<string, int> _questCount;

        private readonly Dictionary<int, Quest> _quests;

        public QuestManager()
        {
            _quests = new Dictionary<int, Quest>();
            _questCount = new Dictionary<string, int>();
            Init();
        }

        public void Init()
        {
            if (_quests.Count > 0)
            {
                _quests.Clear();
            }
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `id`,`type`,`level_num`,`goal_type`,`goal_data`,`action`,`pixel_reward`,`data_bit`,`reward_type`,`timestamp_unlock`,`timestamp_lock` FROM `quests`");
                var dTable = dbClient.GetTable();
                if (dTable != null)
                {
                    foreach (DataRow dRow in dTable.Rows)
                    {
                        var id = Convert.ToInt32(dRow["id"]);
                        var category = Convert.ToString(dRow["type"]);
                        var num = Convert.ToInt32(dRow["level_num"]);
                        var type = Convert.ToInt32(dRow["goal_type"]);
                        var goalData = Convert.ToInt32(dRow["goal_data"]);
                        var name = Convert.ToString(dRow["action"]);
                        var reward = Convert.ToInt32(dRow["pixel_reward"]);
                        var dataBit = Convert.ToString(dRow["data_bit"]);
                        var rewardtype = Convert.ToInt32(dRow["reward_type"].ToString());
                        var time = Convert.ToInt32(dRow["timestamp_unlock"]);
                        var locked = Convert.ToInt32(dRow["timestamp_lock"]);
                        _quests.Add(id,
                            new Quest(id, category, num, (QuestType) type, goalData, name, reward, dataBit, rewardtype, time,
                                locked));
                        AddToCounter(category);
                    }
                }
            }

            log.Info("Quest Manager -> LOADED");
        }

        private void AddToCounter(string category)
        {
            var count = 0;
            if (_questCount.TryGetValue(category, out count))
            {
                _questCount[category] = count + 1;
            }
            else
            {
                _questCount.Add(category, 1);
            }
        }

        public Quest GetQuest(int Id)
        {
            Quest quest = null;
            _quests.TryGetValue(Id, out quest);
            return quest;
        }

        public int GetAmountOfQuestsInCategory(string Category)
        {
            var count = 0;
            _questCount.TryGetValue(Category, out count);
            return count;
        }

        public void ProgressUserQuest(GameClient Session, QuestType QuestType, int EventData = 0)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetStats().QuestID <= 0)
            {
                return;
            }

            var UserQuest = GetQuest(Session.GetHabbo().GetStats().QuestID);
            if (UserQuest == null || UserQuest.GoalType != QuestType)
            {
                return;
            }

            var CurrentProgress = Session.GetHabbo().GetQuestProgress(UserQuest.Id);
            var NewProgress = CurrentProgress;
            var PassQuest = false;
            switch (QuestType)
            {
                default:
                    NewProgress++;
                    if (NewProgress >= UserQuest.GoalData)
                    {
                        PassQuest = true;
                    }
                    break;
                case QuestType.ExploreFindItem:
                    if (EventData != UserQuest.GoalData)
                    {
                        return;
                    }

                    NewProgress = Convert.ToInt32(UserQuest.GoalData);
                    PassQuest = true;
                    break;
                case QuestType.StandOn:
                    if (EventData != UserQuest.GoalData)
                    {
                        return;
                    }

                    NewProgress = Convert.ToInt32(UserQuest.GoalData);
                    PassQuest = true;
                    break;
                case QuestType.XmasParty:
                    NewProgress++;
                    if (NewProgress == UserQuest.GoalData)
                    {
                        PassQuest = true;
                    }
                    break;
                case QuestType.GiveItem:
                    if (EventData != UserQuest.GoalData)
                    {
                        return;
                    }

                    NewProgress = Convert.ToInt32(UserQuest.GoalData);
                    PassQuest = true;
                    break;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_quests` SET `progress` = '" +
                                  NewProgress +
                                  "' WHERE `user_id` = '" +
                                  Session.GetHabbo().Id +
                                  "' AND `quest_id` = '" +
                                  UserQuest.Id +
                                  "' LIMIT 1");
                if (PassQuest)
                {
                    dbClient.RunQuery("UPDATE `user_stats` SET `quest_id` = '0' WHERE `id` = '" + Session.GetHabbo().Id +
                                      "' LIMIT 1");
                }
            }
            Session.GetHabbo().Quests[Session.GetHabbo().GetStats().QuestID] = NewProgress;
            Session.SendPacket(new QuestStartedComposer(Session, UserQuest));
            if (PassQuest)
            {
                Session.GetHabbo()
                    .GetMessenger()
                    .BroadcastAchievement(Session.GetHabbo().Id, MessengerEventTypes.QuestCompleted,
                        UserQuest.Category + "." + UserQuest.Name);
                Session.GetHabbo().GetStats().QuestID = 0;
                Session.GetHabbo().QuestLastCompleted = UserQuest.Id;
                Session.SendPacket(new QuestCompletedComposer(Session, UserQuest));
                Session.GetHabbo().Duckets += UserQuest.Reward;
                Session.SendPacket(new HabboActivityPointNotificationComposer(Session.GetHabbo().Duckets, UserQuest.Reward));
                GetList(Session, null);
            }
        }

        public Quest GetNextQuestInSeries(string Category, int Number)
        {
            foreach (var Quest in _quests.Values)
            {
                if (Quest.Category == Category && Quest.Number == Number)
                {
                    return Quest;
                }
            }

            return null;
        }

        public void GetList(GameClient Session, ClientPacket Message)
        {
            var UserQuestGoals = new Dictionary<string, int>();
            var UserQuests = new Dictionary<string, Quest>();
            foreach (var Quest in _quests.Values.ToList())
            {
                if (Quest.Category.Contains("xmas2012"))
                {
                    continue;
                }

                if (!UserQuestGoals.ContainsKey(Quest.Category))
                {
                    UserQuestGoals.Add(Quest.Category, 1);
                    UserQuests.Add(Quest.Category, null);
                }
                if (Quest.Number >= UserQuestGoals[Quest.Category])
                {
                    var UserProgress = Session.GetHabbo().GetQuestProgress(Quest.Id);
                    if (Session.GetHabbo().GetStats().QuestID != Quest.Id && UserProgress >= Quest.GoalData)
                    {
                        UserQuestGoals[Quest.Category] = Quest.Number + 1;
                    }
                }
            }
            foreach (var Quest in _quests.Values.ToList())
            {
                foreach (var Goal in UserQuestGoals)
                {
                    if (Quest.Category.Contains("xmas2012"))
                    {
                        continue;
                    }

                    if (Quest.Category == Goal.Key && Quest.Number == Goal.Value)
                    {
                        UserQuests[Goal.Key] = Quest;
                        break;
                    }
                }
            }

            Session.SendPacket(new QuestListComposer(Session, Message != null, UserQuests));
        }

        public void QuestReminder(GameClient Session, int QuestId)
        {
            var Quest = GetQuest(QuestId);
            if (Quest == null)
            {
                return;
            }

            Session.SendPacket(new QuestStartedComposer(Session, Quest));
        }
    }
}