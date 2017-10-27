namespace Plus.Communication.Packets.Outgoing.Groups
{
    internal class RefreshFavouriteGroupComposer : ServerPacket
    {
        public RefreshFavouriteGroupComposer(int id)
            : base(ServerPacketHeader.RefreshFavouriteGroupMessageComposer)
        {
            WriteInteger(id);
        }
    }
}