namespace Plus.Communication.Packets.Outgoing.Pets
{
    using HabboHotel.Rooms;
    using HabboHotel.Rooms.AI;
    using HabboHotel.Users;

    internal class RespectPetNotificationMessageComposer : ServerPacket
    {
        public RespectPetNotificationMessageComposer(Pet pet)
            : base(ServerPacketHeader.RespectPetNotificationMessageComposer)
        {
            //TODO: Structure
            WriteInteger(pet.VirtualId);
            WriteInteger(pet.VirtualId);
            WriteInteger(pet.PetId); //Pet Id, 100%
            WriteString(pet.Name);
            WriteInteger(0);
            WriteInteger(0);
            WriteString(pet.Color);
            WriteInteger(0);
            WriteInteger(0); //Count - 3 ints.
            WriteInteger(1);
        }

        public RespectPetNotificationMessageComposer(Habbo habbo, RoomUser user)
            : base(ServerPacketHeader.RespectPetNotificationMessageComposer)
        {
            //TODO: Structure
            WriteInteger(user.VirtualId);
            WriteInteger(user.VirtualId);
            WriteInteger(habbo.Id); //Pet Id, 100%
            WriteString(habbo.Username);
            WriteInteger(0);
            WriteInteger(0);
            WriteString("FFFFFF"); //Yeah..
            WriteInteger(0);
            WriteInteger(0); //Count - 3 ints.
            WriteInteger(1);
        }
    }
}