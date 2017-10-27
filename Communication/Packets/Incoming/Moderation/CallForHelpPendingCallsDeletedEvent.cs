namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;
    using Outgoing.Moderation;

    internal class CallForHelpPendingCallsDeletedEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
            {
                return;
            }

            if (!PlusEnvironment.GetGame().GetModerationManager().UserHasTickets(session.GetHabbo().Id))
            {
                return;
            }

            var pendingTicket = PlusEnvironment.GetGame().GetModerationManager().GetTicketBySenderId(session.GetHabbo().Id);

            if (pendingTicket == null)
            {
                return;
            }

            pendingTicket.Answered = true;
            PlusEnvironment.GetGame().GetClientManager().SendPacket(new ModeratorSupportTicketComposer(session.GetHabbo().Id, pendingTicket), "mod_tool");
        }
    }
}