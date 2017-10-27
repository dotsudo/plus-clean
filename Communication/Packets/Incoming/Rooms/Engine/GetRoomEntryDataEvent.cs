namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    using HabboHotel.GameClients;
    using HabboHotel.Items.Wired;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Chat;
    using Outgoing.Rooms.Engine;

    internal class GetRoomEntryDataEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            if (session.GetHabbo().InRoom)
            {
                Room oldRoom;

                if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out oldRoom))
                {
                    return;
                }

                if (oldRoom.GetRoomUserManager() != null)
                {
                    oldRoom.GetRoomUserManager().RemoveUserFromRoom(session, false);
                }
            }

            if (!room.GetRoomUserManager().AddAvatarToRoom(session))
            {
                room.GetRoomUserManager().RemoveUserFromRoom(session, false);
                return; //TODO: Remove?
            }

            room.SendObjects(session);

            //Status updating for messenger, do later as buggy.

            try
            {
                if (session.GetHabbo().GetMessenger() != null)
                {
                    session.GetHabbo().GetMessenger().OnStatusChanged(true);
                }
            }
            catch
            {
                // ignored
            }

            if (session.GetHabbo().GetStats().QuestID > 0)
            {
                PlusEnvironment.GetGame().GetQuestManager().QuestReminder(session, session.GetHabbo().GetStats().QuestID);
            }

            session.SendPacket(new RoomEntryInfoComposer(room.RoomId, room.CheckRights(session, true)));
            session.SendPacket(new RoomVisualizationSettingsComposer(room.WallThickness, room.FloorThickness, PlusEnvironment.EnumToBool(room.Hidewall.ToString())));

            var thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Username);

            if (thisUser != null && session.GetHabbo().PetId == 0)
            {
                room.SendPacket(new UserChangeComposer(thisUser, false));
            }

            session.SendPacket(new RoomEventComposer(room.RoomData, room.RoomData.Promotion));

            if (room.GetWired() != null)
            {
                room.GetWired().TriggerEvent(WiredBoxType.TriggerRoomEnter, session.GetHabbo());
            }

            if (PlusEnvironment.GetUnixTimestamp() < session.GetHabbo().FloodTime && session.GetHabbo().FloodTime != 0)
            {
                session.SendPacket(new FloodControlComposer((int) session.GetHabbo().FloodTime - (int) PlusEnvironment.GetUnixTimestamp()));
            }
        }
    }
}