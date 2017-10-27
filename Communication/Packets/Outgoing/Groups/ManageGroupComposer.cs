namespace Plus.Communication.Packets.Outgoing.Groups
{
    using HabboHotel.Groups;

    internal class ManageGroupComposer : ServerPacket
    {
        public ManageGroupComposer(Group group, string[] badgeParts)
            : base(ServerPacketHeader.ManageGroupMessageComposer)
        {
            WriteInteger(0);
            WriteBoolean(true);
            WriteInteger(group.Id);
            WriteString(group.Name);
            WriteString(group.Description);
            WriteInteger(1);
            WriteInteger(group.Colour1);
            WriteInteger(group.Colour2);
            WriteInteger(group.GroupType == GroupType.OPEN ? 0 : group.GroupType == GroupType.LOCKED ? 1 : 2);
            WriteInteger(group.AdminOnlyDeco);
            WriteBoolean(false);
            WriteString("");

            WriteInteger(5);

            for (var x = 0; x < badgeParts.Length; x++)
            {
                var symbol = badgeParts[x];

                WriteInteger(symbol.Length >= 6 ? int.Parse(symbol.Substring(0, 3)) : int.Parse(symbol.Substring(0, 2)));
                WriteInteger(symbol.Length >= 6 ? int.Parse(symbol.Substring(3, 2)) : int.Parse(symbol.Substring(2, 2)));
                WriteInteger(symbol.Length < 5 ? 0 : symbol.Length >= 6 ? int.Parse(symbol.Substring(5, 1)) : int.Parse(symbol.Substring(4, 1)));
            }

            var i = 0;
            while (i < 5 - badgeParts.Length)
            {
                WriteInteger(0);
                WriteInteger(0);
                WriteInteger(0);
                i++;
            }

            WriteString(group.Badge);
            WriteInteger(group.MemberCount);
        }
    }
}