namespace Plus.Communication.Packets.Outgoing.Moderation
{
    internal class BroadcastMessageAlertComposer : ServerPacket
    {
        public BroadcastMessageAlertComposer(string message, string url = "")
            : base(ServerPacketHeader.BroadcastMessageAlertMessageComposer)
        {
            WriteString(message);
            WriteString(url);
        }
    }
}