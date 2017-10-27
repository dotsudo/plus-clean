namespace Plus.Communication.Packets.Outgoing.Messenger
{
    using HabboHotel.Users.Messenger;

    internal class FriendNotificationComposer : ServerPacket
    {
        public FriendNotificationComposer(int userId, MessengerEventTypes type, string data)
            : base(ServerPacketHeader.FriendNotificationMessageComposer)
        {
            WriteString(userId.ToString());
            WriteInteger(MessengerEventTypesUtility.GetEventTypePacketNum(type));
            WriteString(data);
        }
    }
}