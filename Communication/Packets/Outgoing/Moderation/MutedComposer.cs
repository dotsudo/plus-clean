namespace Plus.Communication.Packets.Outgoing.Moderation
{
    using System;

    internal class MutedComposer : ServerPacket
    {
        public MutedComposer(double timeMuted)
            : base(ServerPacketHeader.MutedMessageComposer)
        {
            WriteInteger(Convert.ToInt32(timeMuted));
        }
    }
}