﻿namespace Plus.HabboHotel.Items.Wired.Boxes.Triggers
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using Communication.Packets.Incoming;
    using Rooms;
    using Users;

    internal class UserWalksOffBox : IWiredItem
    {
        public UserWalksOffBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            StringData = "";
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.TriggerWalkOffFurni;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket packet)
        {
            packet.PopInt();
            packet.PopString();

            if (SetItems.Count > 0)
            {
                SetItems.Clear();
            }

            var furniCount = packet.PopInt();

            for (var i = 0; i < furniCount; i++)
            {
                var selectedItem = Instance.GetRoomItemHandler().GetItem(Convert.ToInt32(packet.PopInt()));
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

            foreach (var condition in conditions.ToList())
            {
                if (!condition.Execute(player))
                {
                    return false;
                }

                Instance?.GetWired().OnEvent(condition.Item);
            }

            var hasRandomEffectAddon = effects.Count(x => x.Type == WiredBoxType.AddonRandomEffect) > 0;
            if (hasRandomEffectAddon)
            {
                var randomBox = effects.FirstOrDefault(x => x.Type == WiredBoxType.AddonRandomEffect);
                if (randomBox != null && !randomBox.Execute())
                {
                    return false;
                }

                var selectedBox = Instance.GetWired().GetRandomEffect(effects.ToList());
                if (!selectedBox.Execute())
                {
                    return false;
                }

                if (Instance == null)
                {
                    return true;
                }

                Instance.GetWired().OnEvent(randomBox?.Item);
                Instance.GetWired().OnEvent(selectedBox.Item);
            }
            else
            {
                foreach (var effect in effects.ToList())
                {
                    if (!effect.Execute(player))
                    {
                        return false;
                    }

                    Instance?.GetWired().OnEvent(effect.Item);
                }
            }

            return true;
        }
    }
}