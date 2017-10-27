namespace Plus.Communication.Packets.Outgoing.Rooms.Notifications
{
    internal class RoomErrorNotifComposer : ServerPacket
    {
        public RoomErrorNotifComposer(int error)
            : base(ServerPacketHeader.RoomErrorNotifMessageComposer)
        {
            WriteInteger(error);
        }
    }
}