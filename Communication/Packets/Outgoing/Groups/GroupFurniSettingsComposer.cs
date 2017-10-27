namespace Plus.Communication.Packets.Outgoing.Groups
{
    using HabboHotel.Groups;

    internal class GroupFurniSettingsComposer : ServerPacket
    {
        public GroupFurniSettingsComposer(Group group, int itemId, int userId)
            : base(ServerPacketHeader.GroupFurniSettingsMessageComposer)
        {
            WriteInteger(itemId); //Item Id
            WriteInteger(group.Id); //Group Id?
            WriteString(group.Name);
            WriteInteger(group.RoomId); //RoomId
            WriteBoolean(group.IsMember(userId)); //Member?
            WriteBoolean(group.ForumEnabled); //Has a forum
        }
    }
}