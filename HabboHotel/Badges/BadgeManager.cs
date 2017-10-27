namespace Plus.HabboHotel.Badges
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using log4net;

    public class BadgeManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Badges.BadgeManager");

        private readonly Dictionary<string, BadgeDefinition> _badges;

        public BadgeManager() => _badges = new Dictionary<string, BadgeDefinition>();

        public void Init()
        {
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `badge_definitions`;");
                var GetBadges = dbClient.GetTable();
                foreach (DataRow Row in GetBadges.Rows)
                {
                    var BadgeCode = Convert.ToString(Row["code"]).ToUpper();
                    if (!_badges.ContainsKey(BadgeCode))
                    {
                        _badges.Add(BadgeCode, new BadgeDefinition(BadgeCode, Convert.ToString(Row["required_right"])));
                    }
                }
            }

            log.Info("Loaded " + _badges.Count + " badge definitions.");
        }

        public bool TryGetBadge(string BadgeCode, out BadgeDefinition Badge) =>
            _badges.TryGetValue(BadgeCode.ToUpper(), out Badge);
    }
}