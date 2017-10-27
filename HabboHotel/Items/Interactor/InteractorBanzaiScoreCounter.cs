namespace Plus.HabboHotel.Items.Interactor
{
    using System;
    using GameClients;
    using Rooms.Games.Teams;

    public class InteractorBanzaiScoreCounter : IFurniInteractor
    {
        public void OnPlace(GameClient session, Item item)
        {
            if (item.Team == TEAM.NONE)
            {
                return;
            }

            item.ExtraData = item.GetRoom().GetGameManager().Points[Convert.ToInt32(item.Team)].ToString();
            item.UpdateState(false, true);
        }

        public void OnRemove(GameClient session, Item item)
        {
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            if (hasRights)
            {
                item.GetRoom().GetGameManager().Points[Convert.ToInt32(item.Team)] = 0;
                item.ExtraData = "0";
                item.UpdateState();
            }
        }

        public void OnWiredTrigger(Item item)
        {
        }
    }
}