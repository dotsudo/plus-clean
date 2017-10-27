namespace Plus.Communication.Packets.Outgoing.Inventory.Trading
{
    internal class TradingStartComposer : ServerPacket
    {
        public TradingStartComposer(int user1Id, int user2Id)
            : base(ServerPacketHeader.TradingStartMessageComposer)
        {
            WriteInteger(user1Id);
            WriteInteger(1);
            WriteInteger(user2Id);
            WriteInteger(1);
        }
    }
}