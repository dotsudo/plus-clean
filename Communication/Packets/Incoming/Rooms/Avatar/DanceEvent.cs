namespace Plus.Communication.Packets.Incoming.Rooms.Avatar
{
    using HabboHotel.GameClients;
    using HabboHotel.Quests;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Avatar;

    internal class DanceEvent : IPacketEvent
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

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
            {
                return;
            }

            user.UnIdle();

            var danceId = packet.PopInt();
            if (danceId < 0 || danceId > 4)
            {
                danceId = 0;
            }

            if (danceId > 0 && user.CarryItemID > 0)
            {
                user.CarryItem(0);
            }

            if (session.GetHabbo().Effects().CurrentEffect > 0)
            {
                room.SendPacket(new AvatarEffectComposer(user.VirtualId, 0));
            }

            user.DanceId = danceId;

            room.SendPacket(new DanceComposer(user, danceId));

            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.SocialDance);
            if (room.GetRoomUserManager().GetRoomUsers().Count > 19)
            {
                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.MassDance);
            }
        }
    }
}