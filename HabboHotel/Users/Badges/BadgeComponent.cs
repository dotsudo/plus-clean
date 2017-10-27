namespace Plus.HabboHotel.Users.Badges
{
    using System.Collections.Generic;
    using Communication.Packets.Outgoing.Inventory.Badges;
    using Communication.Packets.Outgoing.Inventory.Furni;
    using GameClients;
    using HabboHotel.Badges;
    using UserData;

    public class BadgeComponent
    {
        private readonly Dictionary<string, Badge> _badges;
        private readonly Habbo _player;

        public BadgeComponent(Habbo Player, UserData data)
        {
            _player = Player;
            _badges = new Dictionary<string, Badge>();
            foreach (var badge in data.badges)
            {
                BadgeDefinition BadgeDefinition = null;
                if (!PlusEnvironment.GetGame().GetBadgeManager().TryGetBadge(badge.Code, out BadgeDefinition) ||
                    BadgeDefinition.RequiredRight.Length > 0 && !Player.GetPermissions().HasRight(BadgeDefinition.RequiredRight))
                {
                    continue;
                }

                if (!_badges.ContainsKey(badge.Code))
                {
                    _badges.Add(badge.Code, badge);
                }
            }
        }

        public int Count => _badges.Count;

        public int EquippedCount
        {
            get
            {
                var i = 0;
                foreach (var Badge in _badges.Values)
                {
                    if (Badge.Slot <= 0)
                    {
                        continue;
                    }

                    i++;
                }

                return i;
            }
        }

        public ICollection<Badge> GetBadges() => _badges.Values;

        public Badge GetBadge(string Badge)
        {
            if (_badges.ContainsKey(Badge))
            {
                return _badges[Badge];
            }

            return null;
        }

        public bool TryGetBadge(string BadgeCode, out Badge Badge) => _badges.TryGetValue(BadgeCode, out Badge);

        public bool HasBadge(string Badge) => _badges.ContainsKey(Badge);

        public void GiveBadge(string Badge, bool InDatabase, GameClient Session)
        {
            if (HasBadge(Badge))
            {
                return;
            }

            BadgeDefinition BadgeDefinition = null;
            if (!PlusEnvironment.GetGame().GetBadgeManager().TryGetBadge(Badge.ToUpper(), out BadgeDefinition) ||
                BadgeDefinition.RequiredRight.Length > 0 &&
                !Session.GetHabbo().GetPermissions().HasRight(BadgeDefinition.RequiredRight))
            {
                return;
            }

            if (InDatabase)
            {
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("REPLACE INTO `user_badges` (`user_id`,`badge_id`,`badge_slot`) VALUES ('" +
                                      _player.Id +
                                      "', @badge, '" +
                                      0 +
                                      "')");
                    dbClient.AddParameter("badge", Badge);
                    dbClient.RunQuery();
                }
            }
            _badges.Add(Badge, new Badge(Badge, 0));
            if (Session != null)
            {
                Session.SendPacket(new BadgesComposer(Session));
                Session.SendPacket(new FurniListNotificationComposer(1, 4));
            }
        }

        public void ResetSlots()
        {
            foreach (var Badge in _badges.Values)
            {
                Badge.Slot = 0;
            }
        }

        public void RemoveBadge(string Badge)
        {
            if (!HasBadge(Badge))
            {
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM user_badges WHERE badge_id = @badge AND user_id = " + _player.Id + " LIMIT 1");
                dbClient.AddParameter("badge", Badge);
                dbClient.RunQuery();
            }
            if (_badges.ContainsKey(Badge))
            {
                _badges.Remove(Badge);
            }
        }
    }
}