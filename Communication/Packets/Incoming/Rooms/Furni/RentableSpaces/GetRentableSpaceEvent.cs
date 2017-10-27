namespace Plus.Communication.Packets.Incoming.Rooms.Furni.RentableSpaces
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Furni.RentableSpaces;

    internal class GetRentableSpaceEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var something = packet.PopInt();
            session.SendPacket(new RentableSpaceComposer());
        }
    }
}