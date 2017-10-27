namespace Plus.Communication.Packets.Outgoing.Moderation
{
    using System;
    using HabboHotel.Moderation;
    using Utilities;

    internal class ModeratorSupportTicketComposer : ServerPacket
    {
        public ModeratorSupportTicketComposer(int id, ModerationTicket ticket)
            : base(ServerPacketHeader.ModeratorSupportTicketMessageComposer)
        {
            WriteInteger(ticket.Id); // Id
            WriteInteger(ticket.GetStatus(id)); // Tab ID
            WriteInteger(ticket.Type); // Type
            WriteInteger(ticket.Category); // Category 
            WriteInteger(Convert.ToInt32((DateTime.Now - UnixTimestamp.FromUnixTimestamp(ticket.Timestamp)).TotalMilliseconds)); // This should fix the overflow?
            WriteInteger(ticket.Priority); // Priority
            WriteInteger(0); //??
            WriteInteger(ticket.Sender == null ? 0 : ticket.Sender.Id); // Sender ID

            //base.WriteInteger(1);
            WriteString(ticket.Sender == null ? string.Empty : ticket.Sender.Username); // Sender Name
            WriteInteger(ticket.Reported == null ? 0 : ticket.Reported.Id); // Reported ID
            WriteString(ticket.Reported == null ? string.Empty : ticket.Reported.Username); // Reported Name
            WriteInteger(ticket.Moderator == null ? 0 : ticket.Moderator.Id); // Moderator ID
            WriteString(ticket.Moderator == null ? string.Empty : ticket.Moderator.Username); // Mod Name
            WriteString(ticket.Issue); // Issue
            WriteInteger(ticket.Room == null ? 0 : ticket.Room.Id); // Room Id
            WriteInteger(0);
            {
                // push String
                // push Integer
                // push Integer
            }
        }

        /*public ModeratorSupportTicketComposer(SupportTicket Ticket)
             : base(ServerPacketHeader.ModeratorSupportTicketMessageComposer)
         {
             base.WriteInteger(Ticket.Id);
             base.WriteInteger(Ticket.TabId);
             base.WriteInteger(1); // Type
             base.WriteInteger(114); // Category
             base.WriteInteger(((int)PlusEnvironment.GetUnixTimestamp() - (int)Ticket.Timestamp) * 1000);
             base.WriteInteger(Ticket.Score);
             base.WriteInteger(0);
             base.WriteInteger(Ticket.SenderId);
             base.WriteString(Ticket.SenderName);
             base.WriteInteger(Ticket.ReportedId);
             base.WriteString(Ticket.ReportedName);
             base.WriteInteger((Ticket.Status == TicketStatus.PICKED) ? Ticket.ModeratorId : 0);
             base.WriteString(Ticket.ModName);
             base.WriteString(Ticket.Message);
             base.WriteInteger(0);//No idea?
             base.WriteInteger(0);//String, int, int - this is the "matched to" a string
             {
                 base.WriteString("fresh-hotel.org");
                 base.WriteInteger(-1);
                 base.WriteInteger(-1);
             }
         }*/
    }
}