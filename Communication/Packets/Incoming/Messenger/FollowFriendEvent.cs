﻿namespace Plus.Communication.Packets.Incoming.Messenger
{
    using HabboHotel.GameClients;
    using Outgoing.Messenger;
    using Outgoing.Rooms.Session;

    internal class FollowFriendEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            var buddyId = packet.PopInt();
            if (buddyId == 0 || buddyId == session.GetHabbo().Id)
            {
                return;
            }

            var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(buddyId);
            if (client?.GetHabbo() == null)
            {
                return;
            }

            if (!client.GetHabbo().InRoom)
            {
                session.SendPacket(new FollowFriendFailedComposer(2));
                session.GetHabbo().GetMessenger().UpdateFriend(client.GetHabbo().Id, client, true);
                return;
            }

            if (session.GetHabbo().CurrentRoom != null && client.GetHabbo().CurrentRoom != null)
            {
                if (session.GetHabbo().CurrentRoom.RoomId == client.GetHabbo().CurrentRoom.RoomId)
                {
                    return;
                }
            }

            session.SendPacket(new RoomForwardComposer(client.GetHabbo().CurrentRoomId));
        }
    }
}