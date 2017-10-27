namespace Plus.Communication.Packets.Outgoing.Messenger
{
    internal class FindFriendsProcessResultComposer : ServerPacket
    {
        public FindFriendsProcessResultComposer(bool found)
            : base(ServerPacketHeader.FindFriendsProcessResultMessageComposer)
        {
            WriteBoolean(found);
        }
    }
}