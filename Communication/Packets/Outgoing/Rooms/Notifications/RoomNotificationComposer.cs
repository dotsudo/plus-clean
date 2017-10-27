namespace Plus.Communication.Packets.Outgoing.Rooms.Notifications
{
    internal class RoomNotificationComposer : ServerPacket
    {
        public RoomNotificationComposer(string type, string key, string value)
            : base(ServerPacketHeader.RoomNotificationMessageComposer)
        {
            WriteString(type);
            WriteInteger(1); //Count
            {
                WriteString(key); //Type of message
                WriteString(value);
            }
        }

        public RoomNotificationComposer(string type)
            : base(ServerPacketHeader.RoomNotificationMessageComposer)
        {
            WriteString(type);
            WriteInteger(0); //Count
        }

        public RoomNotificationComposer(string title, string message, string image, string hotelName = "", string hotelUrl = "")
            : base(ServerPacketHeader.RoomNotificationMessageComposer)
        {
            WriteString(image);
            WriteInteger(string.IsNullOrEmpty(hotelName) ? 2 : 4);
            WriteString("title");
            WriteString(title);
            WriteString("message");
            WriteString(message);

            if (string.IsNullOrEmpty(hotelName))
            {
                return;
            }

            WriteString("linkUrl");
            WriteString(hotelUrl);
            WriteString("linkTitle");
            WriteString(hotelName);
        }
    }
}