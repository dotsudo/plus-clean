namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class RoomVisualizationSettingsComposer : ServerPacket
    {
        public RoomVisualizationSettingsComposer(int walls, int floor, bool hideWalls)
            : base(ServerPacketHeader.RoomVisualizationSettingsMessageComposer)
        {
            WriteBoolean(hideWalls);
            WriteInteger(walls);
            WriteInteger(floor);
        }
    }
}