namespace Plus.Communication.Packets.Outgoing.Messenger
{
    internal class FollowFriendFailedComposer : ServerPacket
    {
        public FollowFriendFailedComposer(int errorCode)
            : base(ServerPacketHeader.FollowFriendFailedMessageComposer)
        {
            WriteInteger(errorCode);
        }
    }
}