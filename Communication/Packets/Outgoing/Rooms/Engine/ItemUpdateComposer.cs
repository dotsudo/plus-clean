namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    using HabboHotel.Items;

    internal class ItemUpdateComposer : ServerPacket
    {
        public ItemUpdateComposer(Item item, int userId)
            : base(ServerPacketHeader.ItemUpdateMessageComposer)
        {
            WriteWallItem(item, userId);
        }

        private void WriteWallItem(Item item, int userId)
        {
            WriteString(item.Id.ToString());
            WriteInteger(item.GetBaseItem().SpriteId);
            WriteString(item.WallCoord);
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.Postit:
                    WriteString(item.ExtraData.Split(' ')[0]);
                    break;

                default:
                    WriteString(item.ExtraData);
                    break;
            }

            WriteInteger(-1);
            WriteInteger(item.GetBaseItem().Modes > 1 ? 1 : 0);
            WriteInteger(userId);
        }
    }
}