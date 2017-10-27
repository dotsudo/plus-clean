namespace Plus.Communication.Packets.Incoming.Groups
{
    using System.Collections.Generic;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using Outgoing.Groups;

    internal class GetGroupCreationWindowEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null)
            {
                return;
            }

            var validRooms = new List<RoomData>();
            foreach (var data in session.GetHabbo().UsersRooms)
            {
                if (data.Group == null)
                {
                    validRooms.Add(data);
                }
            }

            session.SendPacket(new GroupCreationWindowComposer(validRooms));
        }
    }
}