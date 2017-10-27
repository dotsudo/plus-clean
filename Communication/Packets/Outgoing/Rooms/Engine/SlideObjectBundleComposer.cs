namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class SlideObjectBundleComposer : ServerPacket
    {
        public SlideObjectBundleComposer(int fromX, int fromY, double fromZ, int toX, int toY, double toZ, int rollerId, int avatarId, int itemId)
            : base(ServerPacketHeader.SlideObjectBundleMessageComposer)
        {
            var isItem = itemId > 0;

            WriteInteger(fromX);
            WriteInteger(fromY);
            WriteInteger(toX);
            WriteInteger(toY);
            WriteInteger(isItem ? 1 : 0);

            if (isItem)
            {
                WriteInteger(itemId);
            }
            else
            {
                WriteInteger(rollerId);
                WriteInteger(2);
                WriteInteger(avatarId);
            }

            WriteDouble(fromZ);
            WriteDouble(toZ);

            if (isItem)
            {
                WriteInteger(rollerId);
            }
        }
    }
}