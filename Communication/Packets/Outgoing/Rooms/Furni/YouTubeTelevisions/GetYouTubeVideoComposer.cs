namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.YouTubeTelevisions
{
    internal class GetYouTubeVideoComposer : ServerPacket
    {
        public GetYouTubeVideoComposer(int itemId, string youTubeVideo)
            : base(ServerPacketHeader.GetYouTubeVideoMessageComposer)
        {
            WriteInteger(itemId);
            WriteString(youTubeVideo); //"9Ht5RZpzPqw");
            WriteInteger(0); //Start seconds
            WriteInteger(0); //End seconds
            WriteInteger(0); //State
        }
    }
}