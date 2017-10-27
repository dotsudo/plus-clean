namespace Plus.Communication.Packets.Outgoing.Navigator
{
    using System.Collections.Generic;
    using HabboHotel.Rooms;

    internal class GuestRoomSearchResultComposer : ServerPacket
    {
        public GuestRoomSearchResultComposer(int mode, string userQuery, ICollection<RoomData> rooms)
            : base(ServerPacketHeader.GuestRoomSearchResultMessageComposer)
        {
            WriteInteger(mode);
            WriteString(userQuery);

            WriteInteger(rooms.Count);
            foreach (var data in rooms)
            {
                RoomAppender.WriteRoom(this, data, data.Promotion);
            }

            WriteBoolean(false);
        }
    }
}