namespace Plus.Communication.Packets.Incoming.Users
{
    using System;
    using System.Linq;
    using HabboHotel.GameClients;
    using Outgoing.Users;

    internal class GetRelationshipsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var habbo = PlusEnvironment.GetHabboById(packet.PopInt());
            if (habbo == null)
            {
                return;
            }

            var rand = new Random();
            habbo.Relationships = habbo.Relationships.OrderBy(x => rand.Next()).ToDictionary(item => item.Key, item => item.Value);

            var loves = habbo.Relationships.Count(x => x.Value.Type == 1);
            var likes = habbo.Relationships.Count(x => x.Value.Type == 2);
            var hates = habbo.Relationships.Count(x => x.Value.Type == 3);

            session.SendPacket(new GetRelationshipsComposer(habbo, loves, likes, hates));
        }
    }
}