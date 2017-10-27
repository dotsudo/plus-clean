namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class RoomEntryInfoComposer : ServerPacket
    {
        public RoomEntryInfoComposer(int roomId, bool isOwner)
            : base(ServerPacketHeader.RoomEntryInfoMessageComposer)
        {
            WriteInteger(roomId);
            WriteBoolean(isOwner);
        }
    }
}