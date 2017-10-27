namespace Plus.HabboHotel.Achievements
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Packets.Outgoing.Inventory.Achievements;
    using Communication.Packets.Outgoing.Inventory.Purse;
    using GameClients;
    using log4net;
    using Users.Messenger;

    public class AchievementManager
    {
        private static readonly ILog Log = LogManager.GetLogger("Plus.HabboHotel.Achievements.AchievementManager");

        public Dictionary<string, Achievement> Achievements;

        public AchievementManager()
        {
            Achievements = new Dictionary<string, Achievement>();
            LoadAchievements();
            Log.Info("Achievement Manager -> LOADED");
        }

        public void LoadAchievements()
        {
            AchievementLevelFactory.GetAchievementLevels(out Achievements);
        }

        public void ProgressAchievement(GameClient session, string achievementGroup, int progressAmount, bool fromZero = false)
        {
            if (!Achievements.ContainsKey(achievementGroup) || session == null)
            {
                return;
            }

            var achievementData = Achievements[achievementGroup];
            var userData = session.GetHabbo().GetAchievementData(achievementGroup);

            if (userData == null)
            {
                userData = new UserAchievement(achievementGroup, 0, 0);
                session.GetHabbo().Achievements.TryAdd(achievementGroup, userData);
            }

            var totalLevels = achievementData.Levels.Count;

            if (userData.Level == totalLevels)
            {
                return;
            }

            var targetLevel = userData?.Level + 1 ?? 1;
            if (targetLevel > totalLevels)
            {
                targetLevel = totalLevels;
            }
            var targetLevelData = achievementData.Levels[targetLevel];
            int newProgress;
            if (fromZero)
            {
                newProgress = progressAmount;
            }
            else
            {
                newProgress = userData?.Progress + progressAmount ?? progressAmount;
            }
            var newLevel = userData?.Level ?? 0;
            var newTarget = newLevel + 1;
            if (newTarget > totalLevels)
            {
                newTarget = totalLevels;
            }
            if (newProgress >= targetLevelData.Requirement)
            {
                newLevel++;
                newTarget++;
                newProgress = 0;
                if (targetLevel == 1)
                {
                    session.GetHabbo().GetBadgeComponent().GiveBadge(achievementGroup + targetLevel, true, session);
                }
                else
                {
                    session.GetHabbo().GetBadgeComponent().RemoveBadge(Convert.ToString(achievementGroup + (targetLevel - 1)));
                    session.GetHabbo().GetBadgeComponent().GiveBadge(achievementGroup + targetLevel, true, session);
                }
                if (newTarget > totalLevels)
                {
                    newTarget = totalLevels;
                }
                session.SendPacket(new AchievementUnlockedComposer(achievementData,
                    targetLevel,
                    targetLevelData.RewardPoints,
                    targetLevelData.RewardPixels));
                session.GetHabbo()
                    .GetMessenger()
                    .BroadcastAchievement(session.GetHabbo().Id, MessengerEventTypes.AchievementUnlocked,
                        achievementGroup + targetLevel);
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("REPLACE INTO `user_achievements` VALUES ('" +
                                      session.GetHabbo().Id +
                                      "', @group, '" +
                                      newLevel +
                                      "', '" +
                                      newProgress +
                                      "')");
                    dbClient.AddParameter("group", achievementGroup);
                    dbClient.RunQuery();
                }
                userData.Level = newLevel;
                userData.Progress = newProgress;
                session.GetHabbo().Duckets += targetLevelData.RewardPixels;
                session.GetHabbo().GetStats().AchievementPoints += targetLevelData.RewardPoints;
                session.SendPacket(
                    new HabboActivityPointNotificationComposer(session.GetHabbo().Duckets, targetLevelData.RewardPixels));
                session.SendPacket(new AchievementScoreComposer(session.GetHabbo().GetStats().AchievementPoints));
                var newLevelData = achievementData.Levels[newTarget];
                session.SendPacket(new AchievementProgressedComposer(achievementData,
                    newTarget,
                    newLevelData,
                    totalLevels,
                    session.GetHabbo().GetAchievementData(achievementGroup)));
                return;
            }

            userData.Level = newLevel;
            userData.Progress = newProgress;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("REPLACE INTO `user_achievements` VALUES ('" +
                                  session.GetHabbo().Id +
                                  "', @group, '" +
                                  newLevel +
                                  "', '" +
                                  newProgress +
                                  "')");
                dbClient.AddParameter("group", achievementGroup);
                dbClient.RunQuery();
            }
            session.SendPacket(new AchievementProgressedComposer(achievementData,
                targetLevel,
                targetLevelData,
                totalLevels,
                session.GetHabbo().GetAchievementData(achievementGroup)));
        }

        internal ICollection<Achievement> GetGameAchievements(int gameId)
        {
            return Achievements.Values.ToList().Where(achievement => achievement.Category == "games" && achievement.GameId == gameId).ToList();
        }
    }
}