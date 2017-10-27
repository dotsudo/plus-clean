namespace Plus.Communication.Packets.Outgoing.Groups
{
    using HabboHotel.Users;

    internal class GroupMemberUpdatedComposer : ServerPacket
    {
        public GroupMemberUpdatedComposer(int groupId, Habbo habbo, int type)
            : base(ServerPacketHeader.GroupMemberUpdatedMessageComposer)
        {
            WriteInteger(groupId); //GroupId
            WriteInteger(type); //Type?
            {
                WriteInteger(habbo.Id); //UserId
                WriteString(habbo.Username);
                WriteString(habbo.Look);
                WriteString(string.Empty);
            }
        }
    }
}