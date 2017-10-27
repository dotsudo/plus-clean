namespace Plus.HabboHotel.Achievements
{
    using System.Collections.Generic;

    public class Achievement
    {
        public string Category;
        public int GameId;
        public string GroupName;
        public int Id;
        public Dictionary<int, AchievementLevel> Levels;

        public Achievement(int Id, string GroupName, string Category, int GameId)
        {
            this.Id = Id;
            this.GroupName = GroupName;
            this.Category = Category;
            this.GameId = GameId;
            Levels = new Dictionary<int, AchievementLevel>();
        }

        public void AddLevel(AchievementLevel Level)
        {
            Levels.Add(Level.Level, Level);
        }
    }
}