namespace Plus.Communication.Packets.Incoming.Groups
{
    using HabboHotel.GameClients;
    using Outgoing.Groups;

    internal class GetBadgeEditorPartsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.SendPacket(new BadgeEditorPartsComposer(
                PlusEnvironment.GetGame().GetGroupManager().BadgeBases,
                PlusEnvironment.GetGame().GetGroupManager().BadgeSymbols,
                PlusEnvironment.GetGame().GetGroupManager().BadgeBaseColours,
                PlusEnvironment.GetGame().GetGroupManager().BadgeSymbolColours,
                PlusEnvironment.GetGame().GetGroupManager().BadgeBackColours));
        }
    }
}