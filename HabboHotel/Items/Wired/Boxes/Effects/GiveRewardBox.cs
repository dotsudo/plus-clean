namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    using System.Collections.Concurrent;
    using Communication.Packets.Incoming;
    using Rooms;

    internal class GiveRewardBox : IWiredItem
    {
        public GiveRewardBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();
            if (SetItems.Count > 0)
            {
                SetItems.Clear();
            }
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.EffectGiveReward;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket packet)
        {
            if (SetItems.Count > 0)
            {
                SetItems.Clear();
            }
            var unknown = packet.PopInt();
            var time = packet.PopInt();
            var message = packet.PopString();

            //this.StringData = Time + ";" + Message;
        }

        public bool Execute(params object[] Params) => true;
    }
}