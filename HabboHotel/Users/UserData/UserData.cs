namespace Plus.HabboHotel.Users.UserData
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Achievements;
    using Badges;
    using Messenger;
    using Relationships;
    using Rooms;

    public class UserData
    {
        public ConcurrentDictionary<string, UserAchievement> achievements;
        public List<Badge> badges;
        public List<int> favouritedRooms;
        public Dictionary<int, MessengerBuddy> friends;
        public Dictionary<int, int> quests;

        public Dictionary<int, Relationship> Relations;
        public Dictionary<int, MessengerRequest> requests;
        public List<RoomData> rooms;
        public Habbo user;
        public int userID;

        public UserData(int userID,
                        ConcurrentDictionary<string, UserAchievement> achievements,
                        List<int> favouritedRooms,
                        List<Badge> badges,
                        Dictionary<int, MessengerBuddy> friends,
                        Dictionary<int, MessengerRequest> requests,
                        List<RoomData> rooms,
                        Dictionary<int, int> quests,
                        Habbo user,
                        Dictionary<int, Relationship> Relations)
        {
            this.userID = userID;
            this.achievements = achievements;
            this.favouritedRooms = favouritedRooms;
            this.badges = badges;
            this.friends = friends;
            this.requests = requests;
            this.rooms = rooms;
            this.quests = quests;
            this.user = user;
            this.Relations = Relations;
        }
    }
}