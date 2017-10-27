namespace Plus.Communication.Packets.Incoming.GameCenter
{
    using HabboHotel.GameClients;
    using Outgoing.GameCenter;

    internal class GetGameListingEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var games = PlusEnvironment.GetGame().GetGameDataManager().GameData;

            session.SendPacket(new GameListComposer(games));
        }
    }
}