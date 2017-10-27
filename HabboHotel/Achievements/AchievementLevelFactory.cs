namespace Plus.HabboHotel.Achievements
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    internal static class AchievementLevelFactory
    {
        internal static void GetAchievementLevels(out Dictionary<string, Achievement> achievements)
        {
            achievements = new Dictionary<string, Achievement>();
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `id`,`category`,`group_name`,`level`,`reward_pixels`,`reward_points`,`progress_needed`,`game_id` FROM `achievements`");
                var dTable = dbClient.GetTable();

                if (dTable == null)
                {
                    return;
                }

                foreach (DataRow dRow in dTable.Rows)
                {
                    var id = Convert.ToInt32(dRow["id"]);
                    var category = Convert.ToString(dRow["category"]);
                    var groupName = Convert.ToString(dRow["group_name"]);
                    var level = Convert.ToInt32(dRow["level"]);
                    var rewardPixels = Convert.ToInt32(dRow["reward_pixels"]);
                    var rewardPoints = Convert.ToInt32(dRow["reward_points"]);
                    var progressNeeded = Convert.ToInt32(dRow["progress_needed"]);
                    var achievementLevel = new AchievementLevel(level, rewardPixels, rewardPoints, progressNeeded);
                    if (!achievements.ContainsKey(groupName))
                    {
                        var achievement = new Achievement(id, groupName, category, Convert.ToInt32(dRow["game_id"]));
                        achievement.AddLevel(achievementLevel);
                        achievements.Add(groupName, achievement);
                    }
                    else
                    {
                        achievements[groupName].AddLevel(achievementLevel);
                    }
                }
            }
        }
    }
}