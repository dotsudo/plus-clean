namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets.Horse
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.AI.Pets;

    internal class ModifyWhoCanRideHorseEvent : IPacketEvent
    {
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

            var petId = packet.PopInt();

            if (!room.GetRoomUserManager().TryGetPet(petId, out var pet))
            {
                return;
            }

            pet.PetData.AnyoneCanRide = pet.PetData.AnyoneCanRide == 1 ? 0 : 1;

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `bots_petdata` SET `anyone_ride` = '" + pet.PetData.AnyoneCanRide + "' WHERE `id` = '" + petId + "' LIMIT 1");
            }

            room.SendPacket(new PetInformationComposer(pet.PetData));
        }
    }
}