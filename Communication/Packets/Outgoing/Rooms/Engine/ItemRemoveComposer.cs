namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    using HabboHotel.Items;

    internal class ItemRemoveComposer : ServerPacket
    {
        public ItemRemoveComposer(Item item, int userId)
            : base(ServerPacketHeader.ItemRemoveMessageComposer)
        {
            WriteString(item.Id.ToString());
            WriteBoolean(false);
            WriteInteger(userId);
        }
    }
}