namespace Plus.Communication.Packets.Outgoing.Rooms.Avatar
{
    using HabboHotel.Rooms;

    internal class DanceComposer : ServerPacket
    {
        public DanceComposer(RoomUser avatar, int dance)
            : base(ServerPacketHeader.DanceMessageComposer)
        {
            WriteInteger(avatar.VirtualId);
            WriteInteger(dance);
        }
    }
}