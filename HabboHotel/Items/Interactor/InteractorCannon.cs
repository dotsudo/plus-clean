﻿namespace Plus.HabboHotel.Items.Interactor
{
    using GameClients;
    using Rooms;

    internal class InteractorCannon : IFurniInteractor
    {
        public void OnPlace(GameClient session, Item item)
        {
        }

        public void OnRemove(GameClient session, Item item)
        {
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            if (session == null || session.GetHabbo() == null || item == null)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            var actor = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (actor == null)
            {
                return;
            }
            if (item.ExtraData == "1")
            {
                return;
            }
            if (Gamemap.TileDistance(actor.X, actor.Y, item.GetX, item.GetY) > 2)
            {
                return;
            }

            item.ExtraData = "1";
            item.UpdateState(false, true);
            item.RequestUpdate(2, true);
        }

        public void OnWiredTrigger(Item item)
        {
            if (item == null)
            {
                return;
            }
            if (item.ExtraData == "1")
            {
                return;
            }

            item.ExtraData = "1";
            item.UpdateState(false, true);
            item.RequestUpdate(2, true);
        }
    }
}