namespace Plus.Communication.Packets.Outgoing.Rooms.Session
{
    public class RoomForwardComposer : ServerPacket
    {
        public RoomForwardComposer(int roomId)
            : base(ServerPacketHeader.RoomForwardMessageComposer)
        {
            WriteInteger(roomId);
        }
    }
}