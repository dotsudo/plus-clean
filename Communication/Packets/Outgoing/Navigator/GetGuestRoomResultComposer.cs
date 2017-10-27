namespace Plus.Communication.Packets.Outgoing.Navigator
{
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;

    internal class GetGuestRoomResultComposer : ServerPacket
    {
        public GetGuestRoomResultComposer(GameClient session, RoomData data, bool isLoading, bool checkEntry)
            : base(ServerPacketHeader.GetGuestRoomResultMessageComposer)
        {
            WriteBoolean(isLoading);
            WriteInteger(data.Id);
            WriteString(data.Name);
            WriteInteger(data.OwnerId);
            WriteString(data.OwnerName);
            WriteInteger(RoomAccessUtility.GetRoomAccessPacketNum(data.Access));
            WriteInteger(data.UsersNow);
            WriteInteger(data.UsersMax);
            WriteString(data.Description);
            WriteInteger(data.TradeSettings);
            WriteInteger(data.Score);
            WriteInteger(0); //Top rated room rank.
            WriteInteger(data.Category);

            WriteInteger(data.Tags.Count);
            foreach (var tag in data.Tags)
            {
                WriteString(tag);
            }

            if (data.Group != null && data.Promotion != null)
            {
                WriteInteger(62); //What?

                WriteInteger(data.Group == null ? 0 : data.Group.Id);
                WriteString(data.Group == null ? "" : data.Group.Name);
                WriteString(data.Group == null ? "" : data.Group.Badge);

                WriteString(data.Promotion != null ? data.Promotion.Name : "");
                WriteString(data.Promotion != null ? data.Promotion.Description : "");
                WriteInteger(data.Promotion != null ? data.Promotion.MinutesLeft : 0);
            }
            else if (data.Group != null && data.Promotion == null)
            {
                WriteInteger(58); //What?
                WriteInteger(data.Group == null ? 0 : data.Group.Id);
                WriteString(data.Group == null ? "" : data.Group.Name);
                WriteString(data.Group == null ? "" : data.Group.Badge);
            }
            else if (data.Group == null && data.Promotion != null)
            {
                WriteInteger(60); //What?
                WriteString(data.Promotion != null ? data.Promotion.Name : "");
                WriteString(data.Promotion != null ? data.Promotion.Description : "");
                WriteInteger(data.Promotion != null ? data.Promotion.MinutesLeft : 0);
            }
            else
            {
                WriteInteger(56); //What?
            }

            WriteBoolean(checkEntry);
            WriteBoolean(false);
            WriteBoolean(false);
            WriteBoolean(false);

            WriteInteger(data.WhoCanMute);
            WriteInteger(data.WhoCanKick);
            WriteInteger(data.WhoCanBan);

            WriteBoolean(session.GetHabbo().GetPermissions().HasRight("mod_tool") || data.OwnerName == session.GetHabbo().Username); //Room muting.
            WriteInteger(data.chatMode);
            WriteInteger(data.chatSize);
            WriteInteger(data.chatSpeed);
            WriteInteger(data.extraFlood); //Hearing distance
            WriteInteger(data.chatDistance); //Flood!!
        }
    }
}