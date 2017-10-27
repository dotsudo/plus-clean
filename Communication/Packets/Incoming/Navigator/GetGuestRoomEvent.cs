namespace Plus.Communication.Packets.Incoming.Navigator
{
    using HabboHotel.GameClients;
    using Outgoing.Navigator;

    internal class GetGuestRoomEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var roomId = packet.PopInt();

            var roomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomId);
            if (roomData == null)
            {
                return;
            }

            var isLoading = packet.PopInt() == 1;
            var checkEntry = packet.PopInt() == 1;

            session.SendPacket(new GetGuestRoomResultComposer(session, roomData, isLoading, checkEntry));
        }
    }
}