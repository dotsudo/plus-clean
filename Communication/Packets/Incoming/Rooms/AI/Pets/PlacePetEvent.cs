namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets
{
    using System;
    using System.Collections.Generic;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using HabboHotel.Rooms.AI;
    using HabboHotel.Rooms.AI.Speech;
    using log4net;
    using Outgoing.Inventory.Pets;
    using Outgoing.Rooms.Notifications;

    internal class PlacePetEvent : IPacketEvent
    {
        private static readonly ILog Log = LogManager.GetLogger("Plus.Communication.Packets.Incoming.Rooms.AI.Pets.PlacePetEvent");

        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                return;
            }

            if (room.AllowPets == 0 && !room.CheckRights(session, true) || !room.CheckRights(session, true))
            {
                session.SendPacket(new RoomErrorNotifComposer(1));
                return;
            }

            if (room.GetRoomUserManager().PetCount > Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("room.pets.placement_limit")))
            {
                session.SendPacket(new RoomErrorNotifComposer(2)); //5 = I have too many.
                return;
            }

            Pet pet;
            if (!session.GetHabbo().GetInventoryComponent().TryGetPet(packet.PopInt(), out pet))
            {
                return;
            }

            if (pet == null)
            {
                return;
            }

            if (pet.PlacedInRoom)
            {
                session.SendNotification("This pet is already in the room?");
                return;
            }

            var x = packet.PopInt();
            var y = packet.PopInt();

            if (!room.GetGameMap().CanWalk(x, y, false))
            {
                session.SendPacket(new RoomErrorNotifComposer(4));
                return;
            }

            RoomUser oldPet;
            if (room.GetRoomUserManager().TryGetPet(pet.PetId, out oldPet))
            {
                room.GetRoomUserManager().RemoveBot(oldPet.VirtualId, false);
            }

            pet.X = x;
            pet.Y = y;

            pet.PlacedInRoom = true;
            pet.RoomId = room.RoomId;

            var rndSpeechList = new List<RandomSpeech>();
            var roomBot = new RoomBot(pet.PetId, pet.RoomId, "pet", "freeroam", pet.Name, "", pet.Look, x, y, 0, 0, 0, 0, 0, 0, ref rndSpeechList, "", 0, pet.OwnerId, false, 0,
                false, 0);
            room.GetRoomUserManager().DeployBot(roomBot, pet);

            pet.DbState = DatabaseUpdateState.NeedsUpdate;
            room.GetRoomUserManager().UpdatePets();

            Pet toRemove;
            if (!session.GetHabbo().GetInventoryComponent().TryRemovePet(pet.PetId, out toRemove))
            {
                Log.Error("Error whilst removing pet: " + toRemove.PetId);
                return;
            }

            session.SendPacket(new PetInventoryComposer(session.GetHabbo().GetInventoryComponent().GetPets()));
        }
    }
}