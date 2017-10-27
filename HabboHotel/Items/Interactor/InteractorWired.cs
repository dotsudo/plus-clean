namespace Plus.HabboHotel.Items.Interactor
{
    using Communication.Packets.Outgoing.Rooms.Furni.Wired;
    using GameClients;
    using Wired;

    public class InteractorWired : IFurniInteractor
    {
        public void OnPlace(GameClient session, Item item)
        {
        }

        public void OnRemove(GameClient session, Item item)
        {
            //Room Room = Item.GetRoom();
            //Room.GetWiredHandler().RemoveWired(Item);
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            if (session == null || item == null)
            {
                return;
            }
            if (!hasRights)
            {
                return;
            }

            IWiredItem box = null;
            if (!item.GetRoom().GetWired().TryGet(item.Id, out box))
            {
                return;
            }

            item.ExtraData = "1";
            item.UpdateState(false, true);
            item.RequestUpdate(2, true);
            if (item.GetBaseItem().WiredType == WiredBoxType.AddonRandomEffect)
            {
                return;
            }

            if (item.GetRoom().GetWired().IsTrigger(item))
            {
                var blockedItems = WiredBoxTypeUtility.ContainsBlockedEffect(box, item.GetRoom().GetWired().GetEffects(box));
                session.SendPacket(new WiredTriggerConfigComposer(box, blockedItems));
            }
            else if (item.GetRoom().GetWired().IsEffect(item))
            {
                var blockedItems = WiredBoxTypeUtility.ContainsBlockedTrigger(box, item.GetRoom().GetWired().GetTriggers(box));
                session.SendPacket(new WiredEffectConfigComposer(box, blockedItems));
            }
            else if (item.GetRoom().GetWired().IsCondition(item))
            {
                session.SendPacket(new WiredConditionConfigComposer(box));
            }
        }

        public void OnWiredTrigger(Item item)
        {
        }
    }
}