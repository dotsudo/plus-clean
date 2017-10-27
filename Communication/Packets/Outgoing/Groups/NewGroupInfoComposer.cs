namespace Plus.Communication.Packets.Outgoing.Groups
{
    internal class NewGroupInfoComposer : ServerPacket
    {
        public NewGroupInfoComposer(int roomId, int groupId)
            : base(ServerPacketHeader.NewGroupInfoMessageComposer)
        {
            WriteInteger(roomId);
            WriteInteger(groupId);
        }
    }
}