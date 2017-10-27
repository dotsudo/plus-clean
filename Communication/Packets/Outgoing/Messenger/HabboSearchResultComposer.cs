namespace Plus.Communication.Packets.Outgoing.Messenger
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.Users.Messenger;

    internal class HabboSearchResultComposer : ServerPacket
    {
        public HabboSearchResultComposer(List<SearchResult> friends, List<SearchResult> otherUsers)
            : base(ServerPacketHeader.HabboSearchResultMessageComposer)
        {
            WriteInteger(friends.Count);
            foreach (var friend in friends.ToList())
            {
                var online = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(friend.UserId) != null;

                WriteInteger(friend.UserId);
                WriteString(friend.Username);
                WriteString(friend.Motto);
                WriteBoolean(online);
                WriteBoolean(false);
                WriteString(string.Empty);
                WriteInteger(0);
                WriteString(online ? friend.Figure : "");
                WriteString(friend.LastOnline);
            }

            WriteInteger(otherUsers.Count);
            foreach (var otherUser in otherUsers.ToList())
            {
                var online = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(otherUser.UserId) != null;

                WriteInteger(otherUser.UserId);
                WriteString(otherUser.Username);
                WriteString(otherUser.Motto);
                WriteBoolean(online);
                WriteBoolean(false);
                WriteString(string.Empty);
                WriteInteger(0);
                WriteString(online ? otherUser.Figure : "");
                WriteString(otherUser.LastOnline);
            }
        }
    }
}