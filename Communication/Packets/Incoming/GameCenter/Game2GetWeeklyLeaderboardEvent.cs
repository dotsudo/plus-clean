namespace Plus.Communication.Packets.Incoming.GameCenter
{
    using HabboHotel.GameClients;

    internal class Game2GetWeeklyLeaderboardEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var gameId = packet.PopInt();

            if (PlusEnvironment.GetGame().GetGameDataManager().TryGetGame(gameId, out _))
            {
                //Code
            }
        }
    }
}