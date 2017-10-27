namespace Plus.Communication.Packets.Outgoing.LandingView
{
    internal class ConcurrentUsersGoalProgressComposer : ServerPacket
    {
        public ConcurrentUsersGoalProgressComposer(int usersNow)
            : base(ServerPacketHeader.ConcurrentUsersGoalProgressMessageComposer)
        {
            WriteInteger(0); //0/1 = Not done, 2 = Done & can claim, 3 = claimed.
            WriteInteger(usersNow);
            WriteInteger(1000);
        }
    }
}