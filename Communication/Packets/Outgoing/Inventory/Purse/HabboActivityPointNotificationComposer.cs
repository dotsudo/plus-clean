namespace Plus.Communication.Packets.Outgoing.Inventory.Purse
{
    internal class HabboActivityPointNotificationComposer : ServerPacket
    {
        public HabboActivityPointNotificationComposer(int balance, int notif, int currencyType = 0)
            : base(ServerPacketHeader.HabboActivityPointNotificationMessageComposer)
        {
            WriteInteger(balance);
            WriteInteger(notif);
            WriteInteger(currencyType); //Type
        }
    }
}