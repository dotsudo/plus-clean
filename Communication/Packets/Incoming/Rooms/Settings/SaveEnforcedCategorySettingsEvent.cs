namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using HabboHotel.GameClients;
    using HabboHotel.Navigator;
    using HabboHotel.Rooms;

    internal class SaveEnforcedCategorySettingsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            Room room = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(packet.PopInt(), out room))
            {
                return;
            }

            if (!room.CheckRights(session, true))
            {
                return;
            }

            var categoryId = packet.PopInt();
            var tradeSettings = packet.PopInt();

            if (tradeSettings < 0 || tradeSettings > 2)
            {
                tradeSettings = 0;
            }

            SearchResultList searchResultList = null;
            if (!PlusEnvironment.GetGame().GetNavigator().TryGetSearchResultList(categoryId, out searchResultList))
            {
                categoryId = 36;
            }

            if (searchResultList.CategoryType != NavigatorCategoryType.CATEGORY || searchResultList.RequiredRank > session.GetHabbo().Rank)
            {
                categoryId = 36;
            }
        }
    }
}