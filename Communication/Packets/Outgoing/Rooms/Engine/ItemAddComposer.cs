namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    using HabboHotel.Items;

    internal class ItemAddComposer : ServerPacket
    {
        public ItemAddComposer(Item item)
            : base(ServerPacketHeader.ItemAddMessageComposer)
        {
            WriteString(item.Id.ToString());
            WriteInteger(item.GetBaseItem().SpriteId);
            WriteString(item.WallCoord ?? string.Empty);

            ItemBehaviourUtility.GenerateWallExtradata(item, this);

            WriteInteger(-1);
            WriteInteger(item.GetBaseItem().Modes > 1 ? 1 : 0); // Type New R63 ('use bottom')
            WriteInteger(item.UserId);
            WriteString(item.Username);
        }
    }
}