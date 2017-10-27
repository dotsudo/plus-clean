namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;
    using Outgoing.Moderation;

    internal class CloseTicketEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            var result = packet.PopInt(); // 1 = useless, 2 = abusive, 3 = resolved

            packet.PopInt();

            var ticketId = packet.PopInt();

            if (!PlusEnvironment.GetGame().GetModerationManager().TryGetTicket(ticketId, out var ticket))
            {
                return;
            }

            if (ticket.Moderator.Id != session.GetHabbo().Id)
            {
                return;
            }

            var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(ticket.Sender.Id);

            client?.SendPacket(new ModeratorSupportTicketResponseComposer(result));

            if (result == 2)
            {
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `user_info` SET `cfhs_abusive` = `cfhs_abusive` + 1 WHERE `user_id` = '" + ticket.Sender.Id + "' LIMIT 1");
                }
            }

            ticket.Answered = true;

            PlusEnvironment.GetGame().GetClientManager().SendPacket(new ModeratorSupportTicketComposer(session.GetHabbo().Id, ticket), "mod_tool");
        }
    }
}