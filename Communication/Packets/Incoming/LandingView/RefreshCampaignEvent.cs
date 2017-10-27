namespace Plus.Communication.Packets.Incoming.LandingView
{
    using HabboHotel.GameClients;
    using Outgoing.LandingView;

    internal class RefreshCampaignEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            try
            {
                var parseCampaings = packet.PopString();

                if (parseCampaings.Contains("gamesmaker"))
                {
                    return;
                }

                var campaingName = "";
                var parser = parseCampaings.Split(';');

                for (var i = 0; i < parser.Length; i++)
                {
                    if (string.IsNullOrEmpty(parser[i]) || parser[i].EndsWith(","))
                    {
                        continue;
                    }

                    var data = parser[i].Split(',');
                    campaingName = data[1];
                }

                session.SendPacket(new CampaignComposer(parseCampaings, campaingName));
            }
            catch
            {
                // ignored
            }
        }
    }
}