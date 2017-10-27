namespace Plus.Communication.Packets.Outgoing.Groups
{
    internal class UnknownGroupComposer : ServerPacket
    {
        public UnknownGroupComposer(int groupId, int habboId)
            : base(ServerPacketHeader.UnknownGroupMessageComposer)
        {
            WriteInteger(groupId);
            WriteInteger(habboId);
        }
    }
}