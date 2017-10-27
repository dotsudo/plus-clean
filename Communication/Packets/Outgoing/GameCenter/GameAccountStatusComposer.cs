namespace Plus.Communication.Packets.Outgoing.GameCenter
{
    internal class GameAccountStatusComposer : ServerPacket
    {
        public GameAccountStatusComposer(int gameId)
            : base(ServerPacketHeader.GameAccountStatusMessageComposer)
        {
            WriteInteger(gameId);
            WriteInteger(-1); // Games Left
            WriteInteger(0); //Was 16?
        }
    }
}