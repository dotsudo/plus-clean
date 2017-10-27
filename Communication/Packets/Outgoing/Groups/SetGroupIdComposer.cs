namespace Plus.Communication.Packets.Outgoing.Groups
{
    internal class SetGroupIdComposer : ServerPacket
    {
        public SetGroupIdComposer(int id)
            : base(ServerPacketHeader.SetGroupIdMessageComposer)
        {
            WriteInteger(id);
        }
    }
}