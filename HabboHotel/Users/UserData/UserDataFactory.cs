namespace Plus.HabboHotel.Users.UserData
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Achievements;
    using Authenticator;
    using Badges;
    using Messenger;
    using Relationships;
    using Rooms;

    public static class UserDataFactory
    {
        public static UserData GetUserData(string sessionTicket, out byte errorCode)
        {
            int userId;
            DataRow dUserInfo;
            DataTable dAchievements;
            DataTable dFavouriteRooms;
            DataTable dBadges;
            DataTable dFriends;
            DataTable dRequests;
            DataTable dRooms;
            DataTable dQuests;
            DataTable dRelations;
            DataRow userInfo;

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `id`,`username`,`rank`,`motto`,`look`,`gender`,`last_online`,`credits`,`activity_points`,`home_room`,`block_newfriends`,`hide_online`,`hide_inroom`,`vip`,`account_created`,`vip_points`,`machine_id`,`volume`,`chat_preference`,`focus_preference`, `pets_muted`,`bots_muted`,`advertising_report_blocked`,`last_change`,`gotw_points`,`ignore_invites`,`time_muted`,`allow_gifts`,`friend_bar_state`,`disable_forced_effects`,`allow_mimic`,`rank_vip` FROM `users` WHERE `auth_ticket` = @sso LIMIT 1");
                dbClient.AddParameter("sso", sessionTicket);
                dUserInfo = dbClient.GetRow();
                if (dUserInfo == null)
                {
                    errorCode = 1;
                    return null;
                }

                userId = Convert.ToInt32(dUserInfo["id"]);
                if (PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(userId) != null)
                {
                    errorCode = 2;
                    PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(userId).Disconnect();
                    return null;
                }

                dbClient.SetQuery("SELECT `group`,`level`,`progress` FROM `user_achievements` WHERE `userid` = '" + userId + "'");
                dAchievements = dbClient.GetTable();
                dbClient.SetQuery("SELECT room_id FROM user_favorites WHERE `user_id` = '" + userId + "'");
                dFavouriteRooms = dbClient.GetTable();
                dbClient.SetQuery("SELECT `badge_id`,`badge_slot` FROM user_badges WHERE `user_id` = '" + userId + "'");
                dBadges = dbClient.GetTable();
                dbClient.SetQuery(
                    "SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online " +
                    "FROM users " +
                    "JOIN messenger_friendships " +
                    "ON users.id = messenger_friendships.user_one_id " +
                    "WHERE messenger_friendships.user_two_id = " +
                    userId +
                    " " +
                    "UNION ALL " +
                    "SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online " +
                    "FROM users " +
                    "JOIN messenger_friendships " +
                    "ON users.id = messenger_friendships.user_two_id " +
                    "WHERE messenger_friendships.user_one_id = " +
                    userId);
                dFriends = dbClient.GetTable();
                dbClient.SetQuery(
                    "SELECT messenger_requests.from_id,messenger_requests.to_id,users.username FROM users JOIN messenger_requests ON users.id = messenger_requests.from_id WHERE messenger_requests.to_id = " +
                    userId);
                dRequests = dbClient.GetTable();
                dbClient.SetQuery("SELECT * FROM rooms WHERE `owner` = '" + userId + "' LIMIT 150");
                dRooms = dbClient.GetTable();
                dbClient.SetQuery("SELECT `quest_id`,`progress` FROM user_quests WHERE `user_id` = '" + userId + "'");
                dQuests = dbClient.GetTable();
                dbClient.SetQuery("SELECT `id`,`user_id`,`target`,`type` FROM `user_relationships` WHERE `user_id` = '" + userId +
                                  "'");
                dRelations = dbClient.GetTable();
                dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + userId + "' LIMIT 1");
                userInfo = dbClient.GetRow();
                if (userInfo == null)
                {
                    dbClient.RunQuery("INSERT INTO `user_info` (`user_id`) VALUES ('" + userId + "')");
                    dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + userId + "' LIMIT 1");
                    userInfo = dbClient.GetRow();
                }
                dbClient.RunQuery("UPDATE `users` SET `online` = '1', `auth_ticket` = '' WHERE `id` = '" + userId + "' LIMIT 1");
            }

            var achievements = new ConcurrentDictionary<string, UserAchievement>();

            foreach (DataRow dRow in dAchievements.Rows)
            {
                achievements.TryAdd(Convert.ToString(dRow["group"]),
                    new UserAchievement(Convert.ToString(dRow["group"]), Convert.ToInt32(dRow["level"]),
                        Convert.ToInt32(dRow["progress"])));
            }

            var favouritedRooms = (from DataRow dRow in dFavouriteRooms.Rows select Convert.ToInt32(dRow["room_id"])).ToList();

            var badges = (from DataRow dRow in dBadges.Rows select new Badge(Convert.ToString(dRow["badge_id"]), Convert.ToInt32(dRow["badge_slot"]))).ToList();

            var friends = new Dictionary<int, MessengerBuddy>();
            foreach (DataRow dRow in dFriends.Rows)
            {
                var friendId = Convert.ToInt32(dRow["id"]);
                var friendName = Convert.ToString(dRow["username"]);
                var friendLook = Convert.ToString(dRow["look"]);
                var friendMotto = Convert.ToString(dRow["motto"]);
                var friendLastOnline = Convert.ToInt32(dRow["last_online"]);
                var friendHideOnline = PlusEnvironment.EnumToBool(dRow["hide_online"].ToString());
                var friendHideRoom = PlusEnvironment.EnumToBool(dRow["hide_inroom"].ToString());

                if (friendId == userId)
                {
                    continue;
                }

                if (!friends.ContainsKey(friendId))
                {
                    friends.Add(friendId,
                        new MessengerBuddy(friendId, friendName, friendLook, friendMotto, friendLastOnline, friendHideOnline,
                            friendHideRoom));
                }
            }

            var requests = new Dictionary<int, MessengerRequest>();
            foreach (DataRow dRow in dRequests.Rows)
            {
                var receiverId = Convert.ToInt32(dRow["from_id"]);
                var senderId = Convert.ToInt32(dRow["to_id"]);
                var requestUsername = Convert.ToString(dRow["username"]);

                if (receiverId != userId)
                {
                    if (!requests.ContainsKey(receiverId))
                    {
                        requests.Add(receiverId, new MessengerRequest(userId, receiverId, requestUsername));
                    }
                }
                else
                {
                    if (!requests.ContainsKey(senderId))
                    {
                        requests.Add(senderId, new MessengerRequest(userId, senderId, requestUsername));
                    }
                }
            }

            var rooms = (from DataRow dRow in dRooms.Rows select PlusEnvironment.GetGame().GetRoomManager().FetchRoomData(Convert.ToInt32(dRow["id"]), dRow)).ToList();

            var quests = new Dictionary<int, int>();

            foreach (DataRow dRow in dQuests.Rows)
            {
                var questId = Convert.ToInt32(dRow["quest_id"]);
                if (quests.ContainsKey(questId))
                {
                    quests.Remove(questId);
                }
                quests.Add(questId, Convert.ToInt32(dRow["progress"]));
            }

            var relationships = new Dictionary<int, Relationship>();

            foreach (DataRow row in dRelations.Rows)
            {
                if (friends.ContainsKey(Convert.ToInt32(row[2])))
                {
                    relationships.Add(Convert.ToInt32(row[2]),
                        new Relationship(Convert.ToInt32(row[0]), Convert.ToInt32(row[2]), Convert.ToInt32(row[3].ToString())));
                }
            }

            var user = HabboFactory.GenerateHabbo(dUserInfo, userInfo);
            errorCode = 0;

            return new UserData(userId, achievements, favouritedRooms, badges, friends, requests, rooms, quests, user,
                relationships);
        }

        internal static UserData GetUserData(int userId)
        {
            DataRow dUserInfo;
            DataRow userInfo;
            DataTable dRelations;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `id`,`username`,`rank`,`motto`,`look`,`gender`,`last_online`,`credits`,`activity_points`,`home_room`,`block_newfriends`,`hide_online`,`hide_inroom`,`vip`,`account_created`,`vip_points`,`machine_id`,`volume`,`chat_preference`, `focus_preference`, `pets_muted`,`bots_muted`,`advertising_report_blocked`,`last_change`,`gotw_points`,`ignore_invites`,`time_muted`,`allow_gifts`,`friend_bar_state`,`disable_forced_effects`,`allow_mimic`,`rank_vip` FROM `users` WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("id", userId);
                dUserInfo = dbClient.GetRow();
                PlusEnvironment.GetGame().GetClientManager().LogClonesOut(Convert.ToInt32(userId));
                if (dUserInfo == null)
                {
                    return null;
                }
                if (PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(userId) != null)
                {
                    return null;
                }

                dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + userId + "' LIMIT 1");
                userInfo = dbClient.GetRow();
                if (userInfo == null)
                {
                    dbClient.RunQuery("INSERT INTO `user_info` (`user_id`) VALUES ('" + userId + "')");
                    dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + userId + "' LIMIT 1");
                    userInfo = dbClient.GetRow();
                }
                dbClient.SetQuery("SELECT group_id,rank FROM group_memberships WHERE user_id=@id");
                dbClient.AddParameter("id", userId);
                dbClient.GetTable();
                dbClient.SetQuery("SELECT `id`,`target`,`type` FROM user_relationships WHERE user_id=@id");
                dbClient.AddParameter("id", userId);
                dRelations = dbClient.GetTable();
            }

            var achievements = new ConcurrentDictionary<string, UserAchievement>();
            var favouritedRooms = new List<int>();
            var badges = new List<Badge>();
            var friends = new Dictionary<int, MessengerBuddy>();
            var friendRequests = new Dictionary<int, MessengerRequest>();
            var rooms = new List<RoomData>();
            var quests = new Dictionary<int, int>();
            var relationships = new Dictionary<int, Relationship>();
            foreach (DataRow row in dRelations.Rows)
            {
                if (!relationships.ContainsKey(Convert.ToInt32(row["id"])))
                {
                    relationships.Add(Convert.ToInt32(row["target"]),
                        new Relationship(Convert.ToInt32(row["id"]), Convert.ToInt32(row["target"]),
                            Convert.ToInt32(row["type"].ToString())));
                }
            }

            var user = HabboFactory.GenerateHabbo(dUserInfo, userInfo);
            return new UserData(userId, achievements, favouritedRooms, badges, friends, friendRequests, rooms, quests, user,
                relationships);
        }
    }
}