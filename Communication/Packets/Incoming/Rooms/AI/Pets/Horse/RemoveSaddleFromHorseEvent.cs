namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets.Horse
{
    using HabboHotel.Catalog.Utilities;
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using HabboHotel.Rooms;
    using Outgoing.Catalog;
    using Outgoing.Inventory.Furni;
    using Outgoing.Rooms.AI.Pets;
    using Outgoing.Rooms.Engine;

    internal class RemoveSaddleFromHorseEvent : IPacketEvent
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

            RoomUser petUser;
            if (!room.GetRoomUserManager().TryGetPet(packet.PopInt(), out petUser))
            {
                return;
            }

            if (petUser.PetData == null || petUser.PetData.OwnerId != session.GetHabbo().Id)
            {
                return;
            }

            var saddleId = ItemUtility.GetSaddleId(petUser.PetData.Saddle);

            petUser.PetData.Saddle = 0;

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `bots_petdata` SET `have_saddle` = '0' WHERE `id` = '" + petUser.PetData.PetId + "' LIMIT 1");
            }

            ItemData itemData;
            if (!PlusEnvironment.GetGame().GetItemManager().GetItem(saddleId, out itemData))
            {
                return;
            }

            var item = ItemFactory.CreateSingleItemNullable(itemData, session.GetHabbo(), "");
            if (item != null)
            {
                session.GetHabbo().GetInventoryComponent().TryAddItem(item);
                session.SendPacket(new FurniListNotificationComposer(item.Id, 1));
                session.SendPacket(new PurchaseOkComposer());
                session.SendPacket(new FurniListAddComposer(item));
                session.SendPacket(new FurniListUpdateComposer());
            }

            room.SendPacket(new UsersComposer(petUser));
            room.SendPacket(new PetHorseFigureInformationComposer(petUser));
        }
    }
}