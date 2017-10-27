namespace Plus.Communication.Packets.Incoming.Rooms.Avatar
{
    using System;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;

    internal class ApplySignEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var signId = packet.PopInt();
            Room room;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out room))
            {
                return;
            }

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
            {
                return;
            }

            user.UnIdle();

            user.SetStatus("sign", Convert.ToString(signId));
            user.UpdateNeeded = true;
            user.SignTime = PlusEnvironment.GetUnixTimestamp() + 5;
        }
    }
}