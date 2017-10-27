namespace Plus.Communication.Packets.Outgoing.Inventory.Trading
{
    internal class TradingErrorComposer : ServerPacket
    {
        public TradingErrorComposer(int error, string username)
            : base(ServerPacketHeader.TradingErrorMessageComposer)
        {
            WriteInteger(error);
            WriteString(username);
        }
    }
}