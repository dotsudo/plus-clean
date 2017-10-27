﻿namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    using System.Collections.Concurrent;
    using Communication.Packets.Incoming;
    using Communication.Packets.Outgoing.Rooms.Chat;
    using Rooms;
    using Users;

    internal class ShowMessageBox : IWiredItem
    {
        public ShowMessageBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }

        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectShowMessage;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }

        public string StringData { get; set; }

        public bool BoolData { get; set; }

        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket packet)
        {
            var unknown = packet.PopInt();
            var message = packet.PopString();
            StringData = message;
        }

        public bool Execute(params object[] Params)
        {
            if (Params == null || Params.Length == 0)
            {
                return false;
            }

            var player = (Habbo) Params[0];
            if (player == null || player.GetClient() == null || string.IsNullOrWhiteSpace(StringData))
            {
                return false;
            }

            var user = player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(player.Username);
            if (user == null)
            {
                return false;
            }

            var message = StringData;
            if (StringData.Contains("%USERNAME%"))
            {
                message = message.Replace("%USERNAME%", player.Username);
            }
            if (StringData.Contains("%ROOMNAME%"))
            {
                message = message.Replace("%ROOMNAME%", player.CurrentRoom.Name);
            }
            if (StringData.Contains("%USERCOUNT%"))
            {
                message = message.Replace("%USERCOUNT%", player.CurrentRoom.UserCount.ToString());
            }
            if (StringData.Contains("%USERSONLINE%"))
            {
                message = message.Replace("%USERSONLINE%", PlusEnvironment.GetGame().GetClientManager().Count.ToString());
            }
            player.GetClient().SendPacket(new WhisperComposer(user.VirtualId, message, 0, 34));
            return true;
        }
    }
}