﻿namespace Plus.Communication.Packets.Incoming.Navigator
{
    using HabboHotel.GameClients;
    using Outgoing.Navigator;

    internal class GetNavigatorFlatsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var categories = PlusEnvironment.GetGame().GetNavigator().GetEventCategories();

            session.SendPacket(new NavigatorFlatCatsComposer(categories));
        }
    }
}