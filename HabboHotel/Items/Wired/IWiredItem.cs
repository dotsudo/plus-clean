namespace Plus.HabboHotel.Items.Wired
{
    using System.Collections.Concurrent;
    using Communication.Packets.Incoming;

    public interface IWiredItem
    {
        Item Item { get; }
        WiredBoxType Type { get; }
        ConcurrentDictionary<int, Item> SetItems { get; set; }
        string StringData { get; set; }
        bool BoolData { get; set; }
        string ItemsData { get; set; }
        void HandleSave(ClientPacket packet);
        bool Execute(params object[] Params);
    }
}