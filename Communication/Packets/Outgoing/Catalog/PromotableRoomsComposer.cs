namespace Plus.Communication.Packets.Outgoing.Catalog
{
    using System.Collections.Generic;
    using HabboHotel.Rooms;

    internal class PromotableRoomsComposer : ServerPacket
    {
        public PromotableRoomsComposer(ICollection<RoomData> rooms)
            : base(ServerPacketHeader.PromotableRoomsMessageComposer)
        {
            WriteBoolean(true);
            WriteInteger(rooms.Count); //Count

            foreach (var data in rooms)
            {
                WriteInteger(data.Id);
                WriteString(data.Name);
                WriteBoolean(false);
            }
        }
    }
}