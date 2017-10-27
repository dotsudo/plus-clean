namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using HabboHotel.Quests;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Engine;

    internal class ApplyDecorationEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            Room room = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out room))
            {
                return;
            }

            if (!room.CheckRights(session, true))
            {
                return;
            }

            var item = session.GetHabbo().GetInventoryComponent().GetItem(packet.PopInt());
            if (item == null)
            {
                return;
            }

            if (item.GetBaseItem() == null)
            {
                return;
            }

            var decorationKey = string.Empty;
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.Floor:
                    decorationKey = "floor";
                    break;

                case InteractionType.Wallpaper:
                    decorationKey = "wallpaper";
                    break;

                case InteractionType.Landscape:
                    decorationKey = "landscape";
                    break;
            }

            switch (decorationKey)
            {
                case "floor":
                    room.Floor = item.ExtraData;
                    room.RoomData.Floor = item.ExtraData;

                    PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.FurniDecorationFloor);
                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_RoomDecoFloor", 1);
                    break;

                case "wallpaper":
                    room.Wallpaper = item.ExtraData;
                    room.RoomData.Wallpaper = item.ExtraData;

                    PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.FurniDecorationWall);
                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_RoomDecoWallpaper", 1);
                    break;

                case "landscape":
                    room.Landscape = item.ExtraData;
                    room.RoomData.Landscape = item.ExtraData;

                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_RoomDecoLandscape", 1);
                    break;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `rooms` SET `" + decorationKey + "` = @extradata WHERE `id` = '" + room.RoomId + "' LIMIT 1");
                dbClient.AddParameter("extradata", item.ExtraData);
                dbClient.RunQuery();

                dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + item.Id + "' LIMIT 1");
            }

            session.GetHabbo().GetInventoryComponent().RemoveItem(item.Id);
            room.SendPacket(new RoomPropertyComposer(decorationKey, item.ExtraData));
        }
    }
}