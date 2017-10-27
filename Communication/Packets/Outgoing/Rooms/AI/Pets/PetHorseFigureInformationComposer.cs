namespace Plus.Communication.Packets.Outgoing.Rooms.AI.Pets
{
    using HabboHotel.Rooms;

    internal class PetHorseFigureInformationComposer : ServerPacket
    {
        public PetHorseFigureInformationComposer(RoomUser petUser)
            : base(ServerPacketHeader.PetHorseFigureInformationMessageComposer)
        {
            WriteInteger(petUser.PetData.VirtualId);
            WriteInteger(petUser.PetData.PetId);
            WriteInteger(petUser.PetData.Type);
            WriteInteger(int.Parse(petUser.PetData.Race));
            WriteString(petUser.PetData.Color.ToLower());
            if (petUser.PetData.Saddle > 0)
            {
                WriteInteger(4);
                WriteInteger(3);
                WriteInteger(3);
                WriteInteger(petUser.PetData.PetHair);
                WriteInteger(petUser.PetData.HairDye);
                WriteInteger(2);
                WriteInteger(petUser.PetData.PetHair);
                WriteInteger(petUser.PetData.HairDye);
                WriteInteger(4);
                WriteInteger(petUser.PetData.Saddle);
                WriteInteger(0);
            }
            else
            {
                WriteInteger(1);
                WriteInteger(2);
                WriteInteger(2);
                WriteInteger(petUser.PetData.PetHair);
                WriteInteger(petUser.PetData.HairDye);
                WriteInteger(3);
                WriteInteger(petUser.PetData.PetHair);
                WriteInteger(petUser.PetData.HairDye);
            }
            WriteBoolean(petUser.PetData.Saddle > 0);
            WriteBoolean(petUser.RidingHorse);
        }
    }
}