namespace Plus.Communication.Packets.Incoming.GameCenter
{
    using HabboHotel.GameClients;

    internal class InitializeGameCenterEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
        }
    }
}