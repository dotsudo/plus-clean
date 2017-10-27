namespace Plus.Communication.Packets.Outgoing.Rooms.Permissions
{
    internal class YouAreControllerComposer : ServerPacket
    {
        public YouAreControllerComposer(int setting)
            : base(ServerPacketHeader.YouAreControllerMessageComposer)
        {
            WriteInteger(setting);
        }
    }
}