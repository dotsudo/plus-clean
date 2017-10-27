namespace Plus.Communication.Packets.Outgoing.Inventory.Achievements
{
    using HabboHotel.Achievements;

    internal class AchievementUnlockedComposer : ServerPacket
    {
        public AchievementUnlockedComposer(Achievement achievement, int level, int pointReward, int pixelReward)
            : base(ServerPacketHeader.AchievementUnlockedMessageComposer)
        {
            WriteInteger(achievement.Id); // Achievement ID
            WriteInteger(level); // Achieved level
            WriteInteger(144); // Unknown. Random useless number.
            WriteString(achievement.GroupName + level); // Achieved name
            WriteInteger(pointReward); // Point reward
            WriteInteger(pixelReward); // Pixel reward
            WriteInteger(0); // Unknown.
            WriteInteger(10); // Unknown.
            WriteInteger(21); // Unknown. (Extra reward?)
            WriteString(level > 1 ? achievement.GroupName + (level - 1) : string.Empty);
            WriteString(achievement.Category);
            WriteBoolean(true);
        }
    }
}