namespace Plus.Communication.Packets.Outgoing.Rooms.Settings
{
    using HabboHotel.Rooms;

    internal class RoomSettingsDataComposer : ServerPacket
    {
        public RoomSettingsDataComposer(Room room)
            : base(ServerPacketHeader.RoomSettingsDataMessageComposer)
        {
            WriteInteger(room.RoomId);
            WriteString(room.Name);
            WriteString(room.Description);
            WriteInteger(RoomAccessUtility.GetRoomAccessPacketNum(room.Access));
            WriteInteger(room.Category);
            WriteInteger(room.UsersMax);
            WriteInteger(room.RoomData.Model.MapSizeX * room.RoomData.Model.MapSizeY > 100 ? 50 : 25);

            WriteInteger(room.Tags.Count);
            foreach (var tag in room.Tags.ToArray())
            {
                WriteString(tag);
            }

            WriteInteger(room.TradeSettings); //Trade
            WriteInteger(room.AllowPets); // allows pets in room - pet system lacking, so always off
            WriteInteger(room.AllowPetsEating); // allows pets to eat your food - pet system lacking, so always off
            WriteInteger(room.RoomBlockingEnabled);
            WriteInteger(room.Hidewall);
            WriteInteger(room.WallThickness);
            WriteInteger(room.FloorThickness);

            WriteInteger(room.chatMode); //Chat mode
            WriteInteger(room.chatSize); //Chat size
            WriteInteger(room.chatSpeed); //Chat speed
            WriteInteger(room.chatDistance); //Hearing Distance
            WriteInteger(room.extraFlood); //Additional Flood

            WriteBoolean(true);

            WriteInteger(room.WhoCanMute); // who can mute
            WriteInteger(room.WhoCanKick); // who can kick
            WriteInteger(room.WhoCanBan); // who can ban
        }
    }
}