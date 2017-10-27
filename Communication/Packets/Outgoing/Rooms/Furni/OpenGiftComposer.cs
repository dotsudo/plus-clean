namespace Plus.Communication.Packets.Outgoing.Rooms.Furni
{
    using HabboHotel.Items;

    internal class OpenGiftComposer : ServerPacket
    {
        public OpenGiftComposer(ItemData data, string text, Item item, bool itemIsInRoom)
            : base(ServerPacketHeader.OpenGiftMessageComposer)
        {
            WriteString(data.Type.ToString());
            WriteInteger(data.SpriteId);
            WriteString(data.ItemName);
            WriteInteger(item.Id);
            WriteString(data.Type.ToString());
            WriteBoolean(itemIsInRoom); //Is it in the room?
            WriteString(text);
        }
    }
}