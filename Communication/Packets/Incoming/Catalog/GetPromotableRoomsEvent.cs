namespace Plus.Communication.Packets.Incoming.Catalog
{
    using System.Linq;
    using HabboHotel.GameClients;
    using Outgoing.Catalog;

    internal class GetPromotableRoomsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var rooms = session.GetHabbo().UsersRooms;
            rooms = rooms.Where(x => x.Promotion == null || x.Promotion.TimestampExpires < PlusEnvironment.GetUnixTimestamp()).ToList();
            session.SendPacket(new PromotableRoomsComposer(rooms));
        }
    }
}