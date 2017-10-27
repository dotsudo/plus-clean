namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    using System;
    using System.Collections.Concurrent;
    using System.Drawing;
    using System.Linq;
    using Communication.Packets.Incoming;
    using Rooms;

    internal class BotMovesToFurniBox : IWiredItem
    {
        public BotMovesToFurniBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.EffectBotMovesToFurniBox;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket packet)
        {
            var unknown = packet.PopInt();
            var botName = packet.PopString();
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

            StringData = botName;
        }

        public bool Execute(params object[] Params)
        {
            if (Params == null || Params.Length == 0 || string.IsNullOrEmpty(StringData))
            {
                return false;
            }

            var user = Instance.GetRoomUserManager().GetBotByName(StringData);
            if (user == null)
            {
                return false;
            }

            var rand = new Random();
            var items = SetItems.Values.ToList();
            items = items.OrderBy(x => rand.Next()).ToList();
            if (items.Count == 0)
            {
                return false;
            }

            var item = items.First();
            if (item == null)
            {
                return false;
            }

            if (!Instance.GetRoomItemHandler().GetFloor.Contains(item))
            {
                SetItems.TryRemove(item.Id, out item);
                if (items.Contains(item))
                {
                    items.Remove(item);
                }
                if (SetItems.Count == 0 || items.Count == 0)
                {
                    return false;
                }

                item = items.First();
                if (item == null)
                {
                    return false;
                }
            }

            if (Instance.GetGameMap() == null)
            {
                return false;
            }

            if (user.IsWalking)
            {
                user.ClearMovement(true);
            }
            user.BotData.ForcedMovement = true;
            user.BotData.TargetCoordinate = new Point(item.GetX, item.GetY);
            user.MoveTo(item.GetX, item.GetY);
            return true;
        }
    }
}