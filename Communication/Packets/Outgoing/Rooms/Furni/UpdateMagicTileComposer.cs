namespace Plus.Communication.Packets.Outgoing.Rooms.Furni
{
    using System;

    internal class UpdateMagicTileComposer : ServerPacket
    {
        public UpdateMagicTileComposer(int itemId, int Decimal)
            : base(ServerPacketHeader.UpdateMagicTileMessageComposer)
        {
            WriteInteger(Convert.ToInt32(itemId));
            WriteInteger(Decimal);
        }
    }
}