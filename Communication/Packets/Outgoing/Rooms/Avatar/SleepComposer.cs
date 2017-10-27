namespace Plus.Communication.Packets.Outgoing.Rooms.Avatar
{
    using HabboHotel.Rooms;

    public class SleepComposer : ServerPacket
    {
        public SleepComposer(RoomUser user, bool isSleeping)
            : base(ServerPacketHeader.SleepMessageComposer)
        {
            WriteInteger(user.VirtualId);
            WriteBoolean(isSleeping);
        }
    }
}