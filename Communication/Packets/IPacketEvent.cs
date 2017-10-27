namespace Plus.Communication.Packets
{
    using HabboHotel.GameClients;
    using Incoming;

    internal interface IPacketEvent
    {
        void Parse(GameClient session, ClientPacket packet);
    }
}