namespace Plus.Communication.Packets.Incoming.Navigator
{
    using HabboHotel.GameClients;

    internal class GoToHotelViewEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
            {
                return;
            }

            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var oldRoom))
            {
                return;
            }

            if (oldRoom.GetRoomUserManager() != null)
            {
                oldRoom.GetRoomUserManager().RemoveUserFromRoom(session, true);
            }
        }
    }
}