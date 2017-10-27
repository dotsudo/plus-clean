namespace Plus.Communication.Packets.Outgoing.Catalog
{
    internal class CheckGnomeNameComposer : ServerPacket
    {
        public CheckGnomeNameComposer(string petName, int errorId)
            : base(ServerPacketHeader.CheckGnomeNameMessageComposer)
        {
            WriteInteger(0);
            WriteInteger(errorId);
            WriteString(petName);
        }
    }
}