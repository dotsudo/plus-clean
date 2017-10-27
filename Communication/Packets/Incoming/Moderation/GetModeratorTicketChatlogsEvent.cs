namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;
    using HabboHotel.Moderation;
    using Outgoing.Moderation;

    internal class GetModeratorTicketChatlogsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_tickets"))
            {
                return;
            }

            var ticketId = packet.PopInt();

            ModerationTicket ticket;
            if (!PlusEnvironment.GetGame().GetModerationManager().TryGetTicket(ticketId, out ticket) || ticket.Room == null)
            {
                return;
            }

            var data = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(ticket.Room.Id);
            if (data == null)
            {
                return;
            }

            session.SendPacket(new ModeratorTicketChatlogComposer(ticket, data, ticket.Timestamp));
        }
    }
}