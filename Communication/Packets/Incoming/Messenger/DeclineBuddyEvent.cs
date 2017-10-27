namespace Plus.Communication.Packets.Incoming.Messenger
{
    using HabboHotel.GameClients;

    internal class DeclineBuddyEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            var declineAll = packet.PopBoolean();

            packet.PopInt();

            if (!declineAll)
            {
                var requestId = packet.PopInt();
                session.GetHabbo().GetMessenger().HandleRequest(requestId);
            }
            else
            {
                session.GetHabbo().GetMessenger().HandleAllRequests();
            }
        }
    }
}