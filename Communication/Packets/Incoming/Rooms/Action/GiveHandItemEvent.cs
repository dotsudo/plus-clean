namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    using System;
    using HabboHotel.GameClients;
    using HabboHotel.Quests;
    using HabboHotel.Rooms;

    internal class GiveHandItemEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().InRoom)
            {
                return;
            }

            Room room = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out room))
            {
                return;
            }

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
            {
                return;
            }

            var targetUser = room.GetRoomUserManager().GetRoomUserByHabbo(packet.PopInt());
            if (targetUser == null)
            {
                return;
            }

            if (!(Math.Abs(user.X - targetUser.X) >= 3 || Math.Abs(user.Y - targetUser.Y) >= 3) || session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                if (user.CarryItemID > 0 && user.CarryTimer > 0)
                {
                    if (user.CarryItemID == 8)
                    {
                        PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.GiveCoffee);
                    }
                    targetUser.CarryItem(user.CarryItemID);
                    user.CarryItem(0);
                    targetUser.DanceId = 0;
                }
            }
        }
    }
}