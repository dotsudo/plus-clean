namespace Plus.HabboHotel.Items.Interactor
{
    using GameClients;

    public class InteractorScoreboard : IFurniInteractor
    {
        public void OnPlace(GameClient session, Item item)
        {
        }

        public void OnRemove(GameClient session, Item item)
        {
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            if (!hasRights)
            {
                return;
            }

            var oldValue = 0;
            if (!int.TryParse(item.ExtraData, out oldValue))
            {
            }
            if (request == 1)
            {
                if (item.PendingReset && oldValue > 0)
                {
                    oldValue = 0;
                    item.PendingReset = false;
                }
                else
                {
                    oldValue = oldValue + 60;
                    item.UpdateNeeded = false;
                }
            }
            else if (request == 2)
            {
                item.UpdateNeeded = !item.UpdateNeeded;
                item.PendingReset = true;
            }
            item.ExtraData = oldValue.ToString();
            item.UpdateState();
        }

        public void OnWiredTrigger(Item item)
        {
            var oldValue = 0;
            if (!int.TryParse(item.ExtraData, out oldValue))
            {
            }
            oldValue = oldValue + 60;
            item.UpdateNeeded = false;
            item.ExtraData = oldValue.ToString();
            item.UpdateState();
        }
    }
}