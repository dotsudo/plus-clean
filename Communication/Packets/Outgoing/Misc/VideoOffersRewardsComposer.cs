namespace Plus.Communication.Packets.Outgoing.Misc
{
    internal class VideoOffersRewardsComposer : ServerPacket
    {
        public VideoOffersRewardsComposer(int id, string type, string message)
            : base(ServerPacketHeader.VideoOffersRewardsMessageComposer)
        {
            WriteString(type);
            WriteInteger(id);
            WriteString(message);
            WriteString("");
        }
    }
}