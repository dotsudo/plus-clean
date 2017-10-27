﻿namespace Plus.HabboHotel.Items.Wired.Boxes.Triggers
{
    using System.Collections.Concurrent;
    using System.Linq;
    using Communication.Packets.Incoming;
    using Rooms;
    using Users;

    internal class StateChangesBox : IWiredItem
    {
        public StateChangesBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.TriggerStateChanges;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket packet)
        {
            var unknown = packet.PopInt();
            var unknown2 = packet.PopString();
            if (SetItems.Count > 0)
            {
                SetItems.Clear();
            }
            var furniCount = packet.PopInt();
            for (var i = 0; i < furniCount; i++)
            {
                var selectedItem = Instance.GetRoomItemHandler().GetItem(packet.PopInt());
                if (selectedItem != null)
                {
                    SetItems.TryAdd(selectedItem.Id, selectedItem);
                }
            }
        }

        public bool Execute(params object[] Params)
        {
            var player = (Habbo) Params[0];
            if (player == null)
            {
                return false;
            }

            var item = (Item) Params[1];
            if (item == null)
            {
                return false;
            }
            if (!SetItems.ContainsKey(item.Id))
            {
                return false;
            }

            var effects = Instance.GetWired().GetEffects(this);
            var conditions = Instance.GetWired().GetConditions(this);
            foreach (var condition in conditions)
            {
                if (!condition.Execute(player))
                {
                    return false;
                }

                if (Instance != null)
                {
                    Instance.GetWired().OnEvent(condition.Item);
                }
            }

            //Check the ICollection to find the random addon effect.
            var hasRandomEffectAddon = effects.Count(x => x.Type == WiredBoxType.AddonRandomEffect) > 0;
            if (hasRandomEffectAddon)
            {
                //Okay, so we have a random addon effect, now lets get the IWiredItem and attempt to execute it.
                var randomBox = effects.FirstOrDefault(x => x.Type == WiredBoxType.AddonRandomEffect);
                if (!randomBox.Execute())
                {
                    return false;
                }

                //Success! Let's get our selected box and continue.
                var selectedBox = Instance.GetWired().GetRandomEffect(effects.ToList());
                if (!selectedBox.Execute())
                {
                    return false;
                }

                //Woo! Almost there captain, now lets broadcast the update to the room instance.
                if (Instance != null)
                {
                    Instance.GetWired().OnEvent(randomBox.Item);
                    Instance.GetWired().OnEvent(selectedBox.Item);
                }
            }
            else
            {
                foreach (var effect in effects)
                {
                    if (!effect.Execute(player))
                    {
                        return false;
                    }

                    if (Instance != null)
                    {
                        Instance.GetWired().OnEvent(effect.Item);
                    }
                }
            }

            return true;
        }
    }
}