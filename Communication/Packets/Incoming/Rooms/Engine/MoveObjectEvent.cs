namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using HabboHotel.Quests;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Engine;

    internal class MoveObjectEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var itemId = packet.PopInt();
            if (itemId == 0)
            {
                return;
            }

            Room room;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out room))
            {
                return;
            }

            Item item;
            if (room.Group != null)
            {
                if (!room.CheckRights(session, false, true))
                {
                    item = room.GetRoomItemHandler().GetItem(itemId);
                    if (item == null)
                    {
                        return;
                    }

                    session.SendPacket(new ObjectUpdateComposer(item, room.OwnerId));
                    return;
                }
            }
            else
            {
                if (!room.CheckRights(session))
                {
                    return;
                }
            }

            item = room.GetRoomItemHandler().GetItem(itemId);

            if (item == null)
            {
                return;
            }

            var x = packet.PopInt();
            var y = packet.PopInt();
            var rotation = packet.PopInt();

            if (x != item.GetX || y != item.GetY)
            {
                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.FurniMove);
            }

            if (rotation != item.Rotation)
            {
                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.FurniRotate);
            }

            if (!room.GetRoomItemHandler().SetFloorItem(session, item, x, y, rotation, false, false, true))
            {
                room.SendPacket(new ObjectUpdateComposer(item, room.OwnerId));
                return;
            }

            if (item.GetZ >= 0.1)
            {
                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.FurniStack);
            }
        }
    }
}