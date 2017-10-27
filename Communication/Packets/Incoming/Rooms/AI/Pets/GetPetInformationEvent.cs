namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.AI.Pets;

    internal class GetPetInformationEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var petId = packet.PopInt();

            if (!session.GetHabbo().CurrentRoom.GetRoomUserManager().TryGetPet(petId, out var pet))
            {
                var user = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(petId);

                if (user?.GetClient() == null || user.GetClient().GetHabbo() == null)
                {
                    return;
                }

                session.SendPacket(new PetInformationComposer(user.GetClient().GetHabbo()));
                return;
            }

            if (pet.RoomId != session.GetHabbo().CurrentRoomId || pet.PetData == null)
            {
                return;
            }

            session.SendPacket(new PetInformationComposer(pet.PetData));
        }
    }
}