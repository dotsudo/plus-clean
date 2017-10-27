namespace Plus.Communication.Packets.Incoming.GameCenter
{
    using HabboHotel.GameClients;
    using Outgoing.GameCenter;

    internal class GetPlayableGamesEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var gameId = packet.PopInt();

            session.SendPacket(new GameAccountStatusComposer(gameId));
            session.SendPacket(new PlayableGamesComposer(gameId));
            session.SendPacket(new GameAchievementListComposer(session, PlusEnvironment.GetGame().GetAchievementManager().GetGameAchievements(gameId), gameId));
        }
    }
}