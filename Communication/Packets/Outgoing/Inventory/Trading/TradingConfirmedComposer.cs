namespace Plus.Communication.Packets.Outgoing.Inventory.Trading
{
    internal class TradingConfirmedComposer : ServerPacket
    {
        public TradingConfirmedComposer(int userId, bool confirmed)
            : base(ServerPacketHeader.TradingConfirmedMessageComposer)
        {
            WriteInteger(userId);
            WriteInteger(confirmed ? 1 : 0);
        }
    }
}