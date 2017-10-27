namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets
{
    using System.Drawing;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using Outgoing.Inventory.Pets;
    using Outgoing.Rooms.Engine;

    internal class PickUpPetEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            if (session.GetHabbo()?.GetInventoryComponent() == null)
            {
                return;
            }

            Room Room;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out Room))
            {
                return;
            }

            var petId = packet.PopInt();

            if (!Room.GetRoomUserManager().TryGetPet(petId, out var pet))
            {
                if (!Room.CheckRights(session) && Room.WhoCanKick != 2 && Room.Group == null || Room.Group != null && !Room.CheckRights(session, false, true))
                {
                    return;
                }

                var targetUser = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(petId);

                if (targetUser?.GetClient() == null || targetUser.GetClient().GetHabbo() == null)
                {
                    return;
                }

                targetUser.GetClient().GetHabbo().PetId = 0;

                Room.SendPacket(new UserRemoveComposer(targetUser.VirtualId));
                Room.SendPacket(new UsersComposer(targetUser));
                return;
            }

            if (session.GetHabbo().Id != pet.PetData.OwnerId && !Room.CheckRights(session, true))
            {
                session.SendWhisper("You can only pickup your own pets, to kick a pet you must have room rights.");
                return;
            }

            if (pet.RidingHorse)
            {
                var userRiding = Room.GetRoomUserManager().GetRoomUserByVirtualId(pet.HorseID);

                if (userRiding != null)
                {
                    userRiding.RidingHorse = false;
                    userRiding.ApplyEffect(-1);
                    userRiding.MoveTo(new Point(userRiding.X + 1, userRiding.Y + 1));
                }
                else
                {
                    pet.RidingHorse = false;
                }
            }

            pet.PetData.RoomId = 0;
            pet.PetData.PlacedInRoom = false;

            var pet2 = pet.PetData;

            if (pet2 != null)
            {
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `bots` SET `room_id` = '0', `x` = '0', `Y` = '0', `Z` = '0' WHERE `id` = '" + pet2.PetId + "' LIMIT 1");
                    dbClient.RunQuery("UPDATE `bots_petdata` SET `experience` = '" + pet2.Experience + "', `energy` = '" + pet2.Energy + "', `nutrition` = '" + pet2.Nutrition +
                                      "', `respect` = '" + pet2.Respect + "' WHERE `id` = '" + pet2.PetId + "' LIMIT 1");
                }
            }

            if (pet2.OwnerId != session.GetHabbo().Id)
            {
                var target = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(pet2.OwnerId);

                if (target != null)
                {
                    target.GetHabbo().GetInventoryComponent().TryAddPet(pet.PetData);
                    Room.GetRoomUserManager().RemoveBot(pet.VirtualId, false);

                    target.SendPacket(new PetInventoryComposer(target.GetHabbo().GetInventoryComponent().GetPets()));
                    return;
                }
            }

            Room.GetRoomUserManager().RemoveBot(pet.VirtualId, false);
        }
    }
}