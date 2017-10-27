namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;

    internal class ModerationKickEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_kick"))
            {
                return;
            }

            var userId = packet.PopInt();
            packet.PopString();

            var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(userId);
            if (client?.GetHabbo() == null || client.GetHabbo().CurrentRoomId < 1 || client.GetHabbo().Id == session.GetHabbo().Id)
            {
                return;
            }

            if (client.GetHabbo().Rank >= session.GetHabbo().Rank)
            {
                session.SendNotification(PlusEnvironment.GetLanguageManager().TryGetValue("moderation.kick.disallowed"));
                return;
            }

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                return;
            }

            room.GetRoomUserManager().RemoveUserFromRoom(client, true);
        }
    }
}