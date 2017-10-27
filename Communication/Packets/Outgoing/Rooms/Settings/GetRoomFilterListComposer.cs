namespace Plus.Communication.Packets.Outgoing.Rooms.Settings
{
    using HabboHotel.Rooms;

    internal class GetRoomFilterListComposer : ServerPacket
    {
        public GetRoomFilterListComposer(Room instance)
            : base(ServerPacketHeader.GetRoomFilterListMessageComposer)
        {
            WriteInteger(instance.WordFilterList.Count);
            foreach (var word in instance.WordFilterList)
            {
                WriteString(word);
            }
        }
    }
}