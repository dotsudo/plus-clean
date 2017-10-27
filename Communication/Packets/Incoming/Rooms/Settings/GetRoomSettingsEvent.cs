namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Settings;

    internal class GetRoomSettingsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var room = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(packet.PopInt());
            if (room == null || !room.CheckRights(session, true))
            {
                return;
            }

            session.SendPacket(new RoomSettingsDataComposer(room));
        }
    }
}