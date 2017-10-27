namespace Plus.Communication.Packets.Outgoing.Rooms.Settings
{
    internal class RoomSettingsSavedComposer : ServerPacket
    {
        public RoomSettingsSavedComposer(int roomId)
            : base(ServerPacketHeader.RoomSettingsSavedMessageComposer)
        {
            WriteInteger(roomId);
        }
    }
}