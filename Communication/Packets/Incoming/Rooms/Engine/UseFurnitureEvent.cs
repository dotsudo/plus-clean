namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using HabboHotel.Items.Wired;
    using HabboHotel.Quests;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Engine;
    using Outgoing.Rooms.Furni;

    internal class UseFurnitureEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || !session.GetHabbo().InRoom)
            {
                return;
            }

            Room room;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out room))
            {
                return;
            }

            var itemId = packet.PopInt();
            var item = room.GetRoomItemHandler().GetItem(itemId);
            if (item == null)
            {
                return;
            }

            var hasRights = room.CheckRights(session, false, true);

            if (item.GetBaseItem().InteractionType == InteractionType.Banzaitele)
            {
                return;
            }

            if (item.GetBaseItem().InteractionType == InteractionType.Toner)
            {
                if (!room.CheckRights(session, true))
                {
                    return;
                }

                if (room.TonerData.Enabled == 0)
                {
                    room.TonerData.Enabled = 1;
                }
                else
                {
                    room.TonerData.Enabled = 0;
                }

                room.SendPacket(new ObjectUpdateComposer(item, room.OwnerId));

                item.UpdateState();

                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `room_items_toner` SET `enabled` = '" + room.TonerData.Enabled + "' LIMIT 1");
                }
                return;
            }

            if (item.Data.InteractionType == InteractionType.GnomeBox && item.UserId == session.GetHabbo().Id)
            {
                session.SendPacket(new GnomeBoxComposer(item.Id));
            }

            var toggle = true;
            if (item.GetBaseItem().InteractionType == InteractionType.WfFloorSwitch1 || item.GetBaseItem().InteractionType == InteractionType.WfFloorSwitch2)
            {
                var user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
                if (user == null)
                {
                    return;
                }

                if (!Gamemap.TilesTouching(item.GetX, item.GetY, user.X, user.Y))
                {
                    toggle = false;
                }
            }
            
            var request = packet.PopInt();

            item.Interactor.OnTrigger(session, item, request, hasRights);

            if (toggle)
            {
                item.GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerStateChanges, session.GetHabbo(), item);
            }

            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.ExploreFindItem, item.GetBaseItem().Id);
        }
    }
}