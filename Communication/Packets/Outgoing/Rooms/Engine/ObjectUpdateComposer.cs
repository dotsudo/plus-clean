namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    using HabboHotel.Items;
    using Utilities;

    internal class ObjectUpdateComposer : ServerPacket
    {
        public ObjectUpdateComposer(Item item, int userId)
            : base(ServerPacketHeader.ObjectUpdateMessageComposer)
        {
            WriteInteger(item.Id);
            WriteInteger(item.GetBaseItem().SpriteId);
            WriteInteger(item.GetX);
            WriteInteger(item.GetY);
            WriteInteger(item.Rotation);
            WriteString($"{TextHandling.GetString(item.GetZ):0.00}");
            WriteString(string.Empty);

            if (item.LimitedNo > 0)
            {
                WriteInteger(1);
                WriteInteger(256);
                WriteString(item.ExtraData);
                WriteInteger(item.LimitedNo);
                WriteInteger(item.LimitedTot);
            }
            else
            {
                ItemBehaviourUtility.GenerateExtradata(item, this);
            }

            WriteInteger(-1); // to-do: check
            WriteInteger(item.GetBaseItem().Modes > 1 ? 1 : 0);
            WriteInteger(userId);
        }
    }
}