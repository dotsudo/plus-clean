namespace Plus.Communication.Packets.Outgoing.Rooms.Furni
{
    internal class GnomeBoxComposer : ServerPacket
    {
        public GnomeBoxComposer(int itemId)
            : base(ServerPacketHeader.GnomeBoxMessageComposer)
        {
            WriteInteger(itemId);
        }
    }
}