namespace Plus.HabboHotel.Items.Interactor
{
    using GameClients;
    using Rooms;
    using Rooms.Games.Teams;

    internal class InteractorFreezeTile : IFurniInteractor
    {
        public void OnPlace(GameClient session, Item item)
        {
        }

        public void OnRemove(GameClient session, Item item)
        {
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            if (session == null || !session.GetHabbo().InRoom || item == null || item.InteractingUser > 0)
            {
                return;
            }

            var user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
            {
                return;
            }

            if (user.Team != TEAM.NONE)
            {
                user.FreezeInteracting = true;
                item.InteractingUser = session.GetHabbo().Id;
                if (item.Data.InteractionType == InteractionType.FreezeTileBlock)
                {
                    if (Gamemap.TileDistance(user.X, user.Y, item.GetX, item.GetY) < 2)
                    {
                        item.GetRoom().GetFreeze().onFreezeTiles(item, item.FreezePowerUp);
                    }
                }
            }
        }

        public void OnWiredTrigger(Item item)
        {
        }
    }
}