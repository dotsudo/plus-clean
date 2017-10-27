namespace Plus.Communication.Packets.Outgoing.BuildersClub
{
    internal class BcBorrowedItemsComposer : ServerPacket
    {
        public BcBorrowedItemsComposer()
            : base(ServerPacketHeader.BcBorrowedItemsMessageComposer)
        {
            WriteInteger(0);
        }
    }
}