namespace Plus.Communication.Packets.Outgoing.Rooms.Settings
{
    using HabboHotel.Rooms;

    internal class FlatControllerRemovedComposer : ServerPacket
    {
        public FlatControllerRemovedComposer(Room instance, int userId)
            : base(ServerPacketHeader.FlatControllerRemovedMessageComposer)
        {
            WriteInteger(instance.Id);
            WriteInteger(userId);
        }
    }
}