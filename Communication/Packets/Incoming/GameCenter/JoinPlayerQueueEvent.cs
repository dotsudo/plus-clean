namespace Plus.Communication.Packets.Incoming.GameCenter
{
    using System;
    using System.Text;
    using HabboHotel.GameClients;
    using Outgoing.GameCenter;

    internal class JoinPlayerQueueEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
            {
                return;
            }

            var gameId = packet.PopInt();

            if (!PlusEnvironment.GetGame().GetGameDataManager().TryGetGame(gameId, out var gameData))
            {
                return;
            }

            var ssoTicket = "HABBOON-Fastfood-" + GenerateSso(32) + "-" + session.GetHabbo().Id;

            session.SendPacket(new JoinQueueComposer(gameData.GameId));
            session.SendPacket(new LoadGameComposer(gameData, ssoTicket));
        }

        private string GenerateSso(int length)
        {
            var random = new Random();
            var characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var result = new StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                result.Append(characters[random.Next(characters.Length)]);
            }

            return result.ToString();
        }
    }
}