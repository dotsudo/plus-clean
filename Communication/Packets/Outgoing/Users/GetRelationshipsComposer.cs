namespace Plus.Communication.Packets.Outgoing.Users
{
    using HabboHotel.Users;

    internal class GetRelationshipsComposer : ServerPacket
    {
        public GetRelationshipsComposer(Habbo habbo, int loves, int likes, int hates)
            : base(ServerPacketHeader.GetRelationshipsMessageComposer)
        {
            WriteInteger(habbo.Id);
            WriteInteger(habbo.Relationships.Count); // Count
            foreach (var rel in habbo.Relationships.Values)
            {
                var hHab = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(rel.UserId);
                if (hHab == null)
                {
                    WriteInteger(0);
                    WriteInteger(0);
                    WriteInteger(0); // Their ID
                    WriteString("Placeholder");
                    WriteString("hr-115-42.hd-190-1.ch-215-62.lg-285-91.sh-290-62");
                }
                else
                {
                    WriteInteger(rel.Type);
                    WriteInteger(rel.Type == 1 ? loves : rel.Type == 2 ? likes : hates);
                    WriteInteger(rel.UserId); // Their ID
                    WriteString(hHab.Username);
                    WriteString(hHab.Look);
                }
            }
        }
    }
}