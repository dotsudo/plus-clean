namespace Plus.Communication.Packets.Incoming.Rooms.FloorPlan
{
    using HabboHotel.GameClients;

    internal class InitializeFloorPlanSessionEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
        }
    }
}