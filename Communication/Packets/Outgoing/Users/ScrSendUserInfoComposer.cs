namespace Plus.Communication.Packets.Outgoing.Users
{
    internal class ScrSendUserInfoComposer : ServerPacket
    {
        public ScrSendUserInfoComposer()
            : base(ServerPacketHeader.ScrSendUserInfoMessageComposer)
        {
            var displayMonths = 0;
            var displayDays = 0;

            WriteString("habbo_club");
            WriteInteger(displayDays);
            WriteInteger(2);
            WriteInteger(displayMonths);
            WriteInteger(1);
            WriteBoolean(true); // hc
            WriteBoolean(true); // vip
            WriteInteger(0);
            WriteInteger(0);
            WriteInteger(495);
        }
    }
}