namespace Plus.Communication.Packets.Incoming.Handshake
{
    using HabboHotel.GameClients;

    internal class SsoTicketEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.Rc4Client == null || session.GetHabbo() != null)
            {
                return;
            }

            var sso = packet.PopString();
            if (string.IsNullOrEmpty(sso) || sso.Length < 15)
            {
                return;
            }

            session.TryAuthenticate(sso);
        }
    }
}