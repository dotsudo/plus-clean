namespace Plus.Communication.Packets.Outgoing.Groups
{
    using HabboHotel.Users;

    internal class GroupMembershipRequestedComposer : ServerPacket
    {
        public GroupMembershipRequestedComposer(int groupId, Habbo habbo, int type)
            : base(ServerPacketHeader.GroupMembershipRequestedMessageComposer)
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