namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using Outgoing.Navigator;

    internal class GiveRoomScoreEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            Room room;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out room))
            {
                return;
            }

            if (session.GetHabbo().RatedRooms.Contains(room.RoomId) || room.CheckRights(session, true))
            {
                return;
            }

            var rating = packet.PopInt();
            switch (rating)
            {
                case -1:

                    room.Score--;
                    room.RoomData.Score--;
                    break;

                case 1:

                    room.Score++;
                    room.RoomData.Score++;
                    break;

                default:

                    return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE rooms SET score = '" + room.Score + "' WHERE id = '" + room.RoomId + "' LIMIT 1");
            }

            session.GetHabbo().RatedRooms.Add(room.RoomId);
            session.SendPacket(new RoomRatingComposer(room.Score, !(session.GetHabbo().RatedRooms.Contains(room.RoomId) || room.CheckRights(session, true))));
        }
    }
}