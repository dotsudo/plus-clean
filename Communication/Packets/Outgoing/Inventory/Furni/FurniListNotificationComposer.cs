namespace Plus.Communication.Packets.Outgoing.Inventory.Furni
{
    internal class FurniListNotificationComposer : ServerPacket
    {
        public FurniListNotificationComposer(int id, int type)
            : base(ServerPacketHeader.FurniListNotificationMessageComposer)
        {
            WriteInteger(1);
            WriteInteger(type);
            WriteInteger(1);
            WriteInteger(id);
        }
    }
}