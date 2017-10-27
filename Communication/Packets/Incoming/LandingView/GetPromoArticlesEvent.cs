﻿namespace Plus.Communication.Packets.Incoming.LandingView
{
    using HabboHotel.GameClients;
    using Outgoing.LandingView;

    internal class GetPromoArticlesEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var landingPromotions = PlusEnvironment.GetGame().GetLandingManager().GetPromotionItems();

            session.SendPacket(new PromoArticlesComposer(landingPromotions));
        }
    }
}