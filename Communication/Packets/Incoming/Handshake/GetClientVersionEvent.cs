namespace Plus.Communication.Packets.Incoming.Handshake
{
    using HabboHotel.GameClients;

    internal class GetClientVersionEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var build = packet.PopString();
            PlusEnvironment.SWFRevision = build;
        }
    }
}