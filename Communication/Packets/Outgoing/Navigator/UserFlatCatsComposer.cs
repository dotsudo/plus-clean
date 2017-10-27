namespace Plus.Communication.Packets.Outgoing.Navigator
{
    using System.Collections.Generic;
    using HabboHotel.Navigator;

    internal class UserFlatCatsComposer : ServerPacket
    {
        public UserFlatCatsComposer(ICollection<SearchResultList> categories, int rank)
            : base(ServerPacketHeader.UserFlatCatsMessageComposer)
        {
            WriteInteger(categories.Count);
            foreach (var cat in categories)
            {
                WriteInteger(cat.Id);
                WriteString(cat.PublicName);
                WriteBoolean(cat.RequiredRank <= rank);
                WriteBoolean(false);
                WriteString("");
                WriteString("");
                WriteBoolean(false);
            }
        }
    }
}