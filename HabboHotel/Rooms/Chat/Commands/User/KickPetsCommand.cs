namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    using System.Drawing;
    using System.Linq;
    using Communication.Packets.Outgoing.Inventory.Pets;
    using GameClients;

    internal class KickPetsCommand : IChatCommand
    {
        public string PermissionRequired => "command_kickpets";

        public string Parameters => "";

        public string Description => "Kick all of the pets from the room.";

        public void Execute(GameClient Session, Room room, string[] Params)
        {
            if (!room.CheckRights(Session, true))
            {
                Session.SendWhisper("Oops, only the room owner can run this command!");
                return;
            }

            if (room.GetRoomUserManager().GetPets().Count > 0)
            {
                foreach (var Pet in room.GetRoomUserManager().GetUserList().ToList())
                {
                    if (Pet == null)
                    {
                        continue;
                    }

                    if (Pet.RidingHorse)
                    {
                        var UserRiding = room.GetRoomUserManager().GetRoomUserByVirtualId(Pet.HorseID);
                        if (UserRiding != null)
                        {
                            UserRiding.RidingHorse = false;
                            UserRiding.ApplyEffect(-1);
                            UserRiding.MoveTo(new Point(UserRiding.X + 1, UserRiding.Y + 1));
                        }
                        else
                        {
                            Pet.RidingHorse = false;
                        }
                    }
                    Pet.PetData.RoomId = 0;
                    Pet.PetData.PlacedInRoom = false;
                    var pet = Pet.PetData;
                    if (pet != null)
                    {
                        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery(
                                "UPDATE `bots` SET `room_id` = '0', `x` = '0', `Y` = '0', `Z` = '0' WHERE `id` = '" +
                                pet.PetId +
                                "' LIMIT 1");
                            dbClient.RunQuery("UPDATE `bots_petdata` SET `experience` = '" +
                                              pet.Experience +
                                              "', `energy` = '" +
                                              pet.Energy +
                                              "', `nutrition` = '" +
                                              pet.Nutrition +
                                              "', `respect` = '" +
                                              pet.Respect +
                                              "' WHERE `id` = '" +
                                              pet.PetId +
                                              "' LIMIT 1");
                        }
                    }
                    if (pet.OwnerId != Session.GetHabbo().Id)
                    {
                        var Target = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(pet.OwnerId);
                        if (Target != null)
                        {
                            Target.GetHabbo().GetInventoryComponent().TryAddPet(Pet.PetData);
                            room.GetRoomUserManager().RemoveBot(Pet.VirtualId, false);
                            Target.SendPacket(new PetInventoryComposer(Target.GetHabbo().GetInventoryComponent().GetPets()));
                            return;
                        }
                    }

                    Session.GetHabbo().GetInventoryComponent().TryAddPet(Pet.PetData);
                    room.GetRoomUserManager().RemoveBot(Pet.VirtualId, false);
                    Session.SendPacket(new PetInventoryComposer(Session.GetHabbo().GetInventoryComponent().GetPets()));
                }

                Session.SendWhisper("Success, removed all pets.");
            }
            else
            {
                Session.SendWhisper("Oops, there isn't any pets in here!?");
            }
        }
    }
}