namespace Plus.Communication.Packets.Outgoing.Help
{
    internal class SubmitBullyReportComposer : ServerPacket
    {
        public SubmitBullyReportComposer(int result)
            : base(ServerPacketHeader.SubmitBullyReportMessageComposer)
        {
            WriteInteger(result);
        }
    }
}