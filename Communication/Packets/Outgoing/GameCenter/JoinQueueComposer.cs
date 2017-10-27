namespace Plus.Communication.Packets.Outgoing.GameCenter
{
    internal class JoinQueueComposer : ServerPacket
    {
        public JoinQueueComposer(int gameId)
            : base(ServerPacketHeader.JoinQueueMessageComposer)
        {
            WriteInteger(gameId);
        }
    }
}