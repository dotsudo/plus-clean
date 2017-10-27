namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;
    using HabboHotel.Moderation;
    using Outgoing.Moderation;

    internal class ReleaseTicketEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            var amount = packet.PopInt();

            for (var i = 0; i < amount; i++)
            {
                ModerationTicket ticket;
                if (!PlusEnvironment.GetGame().GetModerationManager().TryGetTicket(packet.PopInt(), out ticket))
                {
                    continue;
                }

                ticket.Moderator = null;
                PlusEnvironment.GetGame().GetClientManager().SendPacket(new ModeratorSupportTicketComposer(session.GetHabbo().Id, ticket), "mod_tool");
            }
        }
    }
}