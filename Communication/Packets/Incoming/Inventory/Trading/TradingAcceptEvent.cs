﻿namespace Plus.Communication.Packets.Incoming.Inventory.Trading
{
    using HabboHotel.GameClients;
    using Outgoing.Inventory.Trading;

    internal class TradingAcceptEvent : IPacketEvent
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

            var tradeUser = trade.Users[0];
            if (tradeUser.RoomUser != roomUser)
            {
                tradeUser = trade.Users[1];
            }

            tradeUser.HasAccepted = true;
            trade.SendPacket(new TradingAcceptComposer(session.GetHabbo().Id, true));

            if (!trade.AllAccepted)
            {
                return;
            }

            trade.SendPacket(new TradingCompleteComposer());
            trade.CanChange = false;
            trade.RemoveAccepted();
        }
    }
}