namespace Plus.Communication.Packets.Outgoing.Rooms.Settings
{
    internal class RoomMuteSettingsComposer : ServerPacket
    {
        public RoomMuteSettingsComposer(bool status)
            : base(ServerPacketHeader.RoomMuteSettingsMessageComposer)
        {
            WriteBoolean(status);
        }
    }
}