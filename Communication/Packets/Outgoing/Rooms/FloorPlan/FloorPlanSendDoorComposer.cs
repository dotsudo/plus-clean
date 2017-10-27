namespace Plus.Communication.Packets.Outgoing.Rooms.FloorPlan
{
    internal class FloorPlanSendDoorComposer : ServerPacket
    {
        public FloorPlanSendDoorComposer(int doorX, int doorY, int doorDirection)
            : base(ServerPacketHeader.FloorPlanSendDoorMessageComposer)
        {
            WriteInteger(doorX);
            WriteInteger(doorY);
            WriteInteger(doorDirection);
        }
    }
}