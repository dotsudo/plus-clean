namespace Plus.Communication.Packets.Outgoing.Inventory.Furni
{
    internal class FurniListRemoveComposer : ServerPacket
    {
        public FurniListRemoveComposer(int id)
            : base(ServerPacketHeader.FurniListRemoveMessageComposer)
        {
            WriteInteger(id);
        }
    }
}