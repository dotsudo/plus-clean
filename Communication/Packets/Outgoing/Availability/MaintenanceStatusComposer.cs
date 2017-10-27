namespace Plus.Communication.Packets.Outgoing.Availability
{
    internal class MaintenanceStatusComposer : ServerPacket
    {
        public MaintenanceStatusComposer(int minutes, int duration)
            : base(ServerPacketHeader.MaintenanceStatusMessageComposer)
        {
            WriteBoolean(false);
            WriteInteger(minutes); //Time till shutdown.
            WriteInteger(duration); //Duration
        }
    }
}