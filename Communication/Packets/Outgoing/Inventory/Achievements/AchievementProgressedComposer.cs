namespace Plus.Communication.Packets.Outgoing.Inventory.Achievements
{
    using HabboHotel.Achievements;

    internal class AchievementProgressedComposer : ServerPacket
    {
        public AchievementProgressedComposer(Achievement achievement, int targetLevel, AchievementLevel targetLevelData, int totalLevels, UserAchievement userData)
            : base(ServerPacketHeader.AchievementProgressedMessageComposer)
        {
            WriteInteger(achievement.Id); // Unknown (ID?)
            WriteInteger(targetLevel); // Target level
            WriteString(achievement.GroupName + targetLevel); // Target name/desc/badge
            WriteInteger(1); // Progress req/target 
            WriteInteger(targetLevelData.Requirement); // Reward in Pixels
            WriteInteger(targetLevelData.RewardPixels); // Reward Ach Score
            WriteInteger(0); // ?
            WriteInteger(userData?.Progress ?? 0); // Current progress
            WriteBoolean(userData != null && userData.Level >= totalLevels); // Set 100% completed(??)
            WriteString(achievement.Category); // Category
            WriteString(string.Empty);
            WriteInteger(totalLevels); // Total amount of levels 
            WriteInteger(0);
        }
    }
}