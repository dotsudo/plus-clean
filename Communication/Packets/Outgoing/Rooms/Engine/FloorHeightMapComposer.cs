namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class FloorHeightMapComposer : ServerPacket
    {
        public FloorHeightMapComposer(string map, int wallHeight)
            : base(ServerPacketHeader.FloorHeightMapMessageComposer)
        {
            WriteBoolean(false);
            WriteInteger(wallHeight);
            WriteString(map);
        }
    }
}