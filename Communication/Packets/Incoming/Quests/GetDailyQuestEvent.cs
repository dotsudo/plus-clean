﻿namespace Plus.Communication.Packets.Incoming.Quests
{
    using HabboHotel.GameClients;
    using Outgoing.LandingView;

    internal class GetDailyQuestEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var usersOnline = PlusEnvironment.GetGame().GetClientManager().Count;

            session.SendPacket(new ConcurrentUsersGoalProgressComposer(usersOnline));
        }
    }
}