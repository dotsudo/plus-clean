namespace Plus.Communication.Packets.Outgoing.Rooms.Session
{
    internal class RoomReadyComposer : ServerPacket
    {
        public RoomReadyComposer(int roomId, string model)
            : base(ServerPacketHeader.RoomReadyMessageComposer)
        {
            WriteString(model);
            WriteInteger(roomId);
        }
    }
}