namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    using HabboHotel.Items;

    internal class ObjectRemoveComposer : ServerPacket
    {
        public ObjectRemoveComposer(Item item, int userId)
            : base(ServerPacketHeader.ObjectRemoveMessageComposer)
        {
            WriteString(item.Id.ToString());
            WriteBoolean(false);
            WriteInteger(userId);
            WriteInteger(0);
        }
    }
}