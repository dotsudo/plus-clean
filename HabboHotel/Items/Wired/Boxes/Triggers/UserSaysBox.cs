﻿namespace Plus.HabboHotel.Items.Wired.Boxes.Triggers
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using Communication.Packets.Incoming;
    using Communication.Packets.Outgoing.Rooms.Chat;
    using Rooms;
    using Users;

    internal class UserSaysBox : IWiredItem
    {
        public UserSaysBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            StringData = "";
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.TriggerUserSays;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket packet)
        {
            var unknown = packet.PopInt();
            var ownerOnly = packet.PopInt();
            var message = packet.PopString();
            BoolData = ownerOnly == 1;
            StringData = message;
        }

        public bool Execute(params object[] Params)
        {
            var player = (Habbo) Params[0];
            if (player == null || player.CurrentRoom == null || !player.InRoom)
            {
                return false;
            }

            var user = player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(player.Username);
            if (user == null)
            {
                return false;
            }

            var message = Convert.ToString(Params[1]);
            if (BoolData && Instance.OwnerId != player.Id ||
                player == null ||
                string.IsNullOrWhiteSpace(message) ||
                string.IsNullOrWhiteSpace(StringData))
            {
                return false;
            }

            if (message.Contains(" " + StringData) || message.Contains(StringData + " ") || message == StringData)
            {
                player.WiredInteraction = true;
                var effects = Instance.GetWired().GetEffects(this);
                var conditions = Instance.GetWired().GetConditions(this);
                foreach (var condition in conditions.ToList())
                {
                    if (!condition.Execute(player))
                    {
                        return false;
                    }

                    Instance.GetWired().OnEvent(condition.Item);
                }

                player.GetClient().SendPacket(new WhisperComposer(user.VirtualId, message, 0, 0));

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
                    foreach (var effect in effects.ToList())
                    {
                        if (!effect.Execute(player))
                        {
                            return false;
                        }

                        Instance.GetWired().OnEvent(effect.Item);
                    }
                }

                return true;
            }

            return false;
        }
    }
}