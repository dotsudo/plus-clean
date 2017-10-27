namespace Plus.Communication.Packets.Incoming.Navigator
{
    using HabboHotel.GameClients;
    using HabboHotel.Navigator;
    using HabboHotel.Rooms;
    using Outgoing.Navigator;

    internal class CreateFlatEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
            {
                return;
            }

            if (session.GetHabbo().UsersRooms.Count >= 500)
            {
                session.SendPacket(new CanCreateRoomComposer(true, 500));
                return;
            }

            var name = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(packet.PopString());
            var description = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(packet.PopString());
            var modelName = packet.PopString();

            var category = packet.PopInt();
            var maxVisitors = packet.PopInt(); //10 = min, 25 = max.
            var tradeSettings = packet.PopInt(); //2 = All can trade, 1 = owner only, 0 = no trading.

            if (name.Length < 3)
            {
                return;
            }

            if (name.Length > 25)
            {
                return;
            }

            RoomModel roomModel = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetModel(modelName, out roomModel))
            {
                return;
            }

            SearchResultList searchResultList = null;
            if (!PlusEnvironment.GetGame().GetNavigator().TryGetSearchResultList(category, out searchResultList))
            {
                category = 36;
            }

            if (searchResultList.CategoryType != NavigatorCategoryType.CATEGORY || searchResultList.RequiredRank > session.GetHabbo().Rank)
            {
                category = 36;
            }

            if (maxVisitors < 10 || maxVisitors > 25)
            {
                maxVisitors = 10;
            }

            if (tradeSettings < 0 || tradeSettings > 2)
            {
                tradeSettings = 0;
            }

            var newRoom = PlusEnvironment.GetGame().GetRoomManager().CreateRoom(session, name, description, modelName, category, maxVisitors, tradeSettings);
            if (newRoom != null)
            {
                session.SendPacket(new FlatCreatedComposer(newRoom.Id, name));
            }
        }
    }
}