﻿namespace Plus.Communication.Packets.Incoming.Inventory.Trading
{
    using HabboHotel.GameClients;
    using Outgoing.Inventory.Trading;

    internal class TradingModifyEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || !session.GetHabbo().InRoom)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;

            var roomUser = room?.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (roomUser == null)
            {
                return;
            }

            if (!room.GetTrading().TryGetTrade(roomUser.TradeId, out var trade))
            {
                session.SendPacket(new TradingClosedComposer(session.GetHabbo().Id));
                return;
            }

            if (!trade.CanChange)
            {
                return;
            }

            var user = trade.Users[0];

            if (user.RoomUser != roomUser)
            {
                user = trade.Users[1];
            }

            user.HasAccepted = false;
            trade.SendPacket(new TradingAcceptComposer(session.GetHabbo().Id, false));
        }
    }
}