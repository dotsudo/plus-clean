namespace Plus.Communication.Packets.Incoming.Catalog
{
    using System;
    using HabboHotel.Catalog.Utilities;
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using Outgoing.Catalog;
    using Outgoing.Inventory.Furni;

    internal class CheckGnomeNameEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || !session.GetHabbo().InRoom)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            var itemId = packet.PopInt();
            var item = room.GetRoomItemHandler().GetItem(itemId);

            if (item?.Data == null || item.UserId != session.GetHabbo().Id || item.Data.InteractionType != InteractionType.GnomeBox)
            {
                return;
            }

            var petName = packet.PopString();
            if (string.IsNullOrEmpty(petName))
            {
                session.SendPacket(new CheckGnomeNameComposer(petName, 1));
                return;
            }

            if (!PlusEnvironment.IsValidAlphaNumeric(petName))
            {
                session.SendPacket(new CheckGnomeNameComposer(petName, 1));
                return;
            }

            //Quickly delete it from the database.
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM `items` WHERE `id` = @ItemId LIMIT 1");
                dbClient.AddParameter("ItemId", item.Id);
                dbClient.RunQuery();
            }

            //Remove the item.
            room.GetRoomItemHandler().RemoveFurniture(session, item.Id);

            //Apparently we need this for success.
            session.SendPacket(new CheckGnomeNameComposer(petName, 0));

            //Create the pet here.
            var pet = PetUtility.CreatePet(session.GetHabbo().Id, petName, 26, "30", "ffffff");
            if (pet == null)
            {
                session.SendNotification("Oops, an error occoured. Please report this!");
                return;
            }

            pet.RoomId = session.GetHabbo().CurrentRoomId;
            pet.GnomeClothing = RandomClothing();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `bots_petdata` SET `gnome_clothing` = @GnomeClothing WHERE `id` = @PetId LIMIT 1");
                dbClient.AddParameter("GnomeClothing", pet.GnomeClothing);
                dbClient.AddParameter("PetId", pet.PetId);
                dbClient.RunQuery();
            }

            ItemData petFood;

            if (!PlusEnvironment.GetGame().GetItemManager().GetItem(320, out petFood))
            {
                return;
            }

            var food = ItemFactory.CreateSingleItemNullable(petFood, session.GetHabbo(), "");

            if (food == null)
            {
                return;
            }

            session.GetHabbo().GetInventoryComponent().TryAddItem(food);
            session.SendPacket(new FurniListNotificationComposer(food.Id, 1));
        }

        private static string RandomClothing()
        {
            var random = new Random();

            var randomNumber = random.Next(1, 6);
            switch (randomNumber)
            {
                default:
                    return "5 0 -1 0 4 402 5 3 301 4 1 101 2 2 201 3";
                case 2:
                    return "5 0 -1 0 1 102 13 3 301 4 4 401 5 2 201 3";
                case 3:
                    return "5 1 102 8 2 201 16 4 401 9 3 303 4 0 -1 6";
                case 4:
                    return "5 0 -1 0 3 303 4 4 401 5 1 101 2 2 201 3";
                case 5:
                    return "5 3 302 4 2 201 11 1 102 12 0 -1 28 4 401 24";
                case 6:
                    return "5 4 402 5 3 302 21 0 -1 7 1 101 12 2 201 17";
            }
        }
    }
}