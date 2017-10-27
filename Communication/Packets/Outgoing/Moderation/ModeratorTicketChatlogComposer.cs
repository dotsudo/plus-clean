﻿namespace Plus.Communication.Packets.Outgoing.Moderation
{
    using HabboHotel.Moderation;
    using HabboHotel.Rooms;
    using Utilities;

    internal class ModeratorTicketChatlogComposer : ServerPacket
    {
        public ModeratorTicketChatlogComposer(ModerationTicket ticket, RoomData roomData, double timestamp)
            : base(ServerPacketHeader.ModeratorTicketChatlogMessageComposer)
        {
            WriteInteger(ticket.Id);
            WriteInteger(ticket.Sender?.Id ?? 0);
            WriteInteger(ticket.Reported != null ? ticket.Reported.Id : 0);
            WriteInteger(roomData.Id);

            WriteByte(1);
            WriteShort(2); //Count
            WriteString("roomName");
            WriteByte(2);
            WriteString(roomData.Name);
            WriteString("roomId");
            WriteByte(1);
            WriteInteger(roomData.Id);

            WriteShort(ticket.ReportedChats.Count);
            foreach (var chat in ticket.ReportedChats)
            {
                WriteString(UnixTimestamp.FromUnixTimestamp(timestamp).ToShortTimeString());
                WriteInteger(ticket.Id);
                WriteString(ticket.Reported != null ? ticket.Reported.Username : "No username");
                WriteString(chat);
                WriteBoolean(false);
            }
        }
    }
}