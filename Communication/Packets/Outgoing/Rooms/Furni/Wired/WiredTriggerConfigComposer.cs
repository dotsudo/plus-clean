namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.Wired
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.Items.Wired;

    internal class WiredTriggerConfigComposer : ServerPacket
    {
        public WiredTriggerConfigComposer(IWiredItem box, List<int> blockedItems)
            : base(ServerPacketHeader.WiredTriggerConfigMessageComposer)
        {
            WriteBoolean(false);
            WriteInteger(5);

            WriteInteger(box.SetItems.Count);
            foreach (var item in box.SetItems.Values.ToList())
            {
                WriteInteger(item.Id);
            }

            WriteInteger(box.Item.GetBaseItem().SpriteId);
            WriteInteger(box.Item.Id);
            WriteString(box.StringData);

            WriteInteger(box is IWiredCycle ? 1 : 0);
            if (box is IWiredCycle)
            {
                var cycle = (IWiredCycle) box;
                WriteInteger(cycle.Delay);
            }
            WriteInteger(0);
            WriteInteger(WiredBoxTypeUtility.GetWiredId(box.Type));
            WriteInteger(blockedItems.Count());
            if (blockedItems.Any())
            {
                foreach (var id in blockedItems.ToList())
                {
                    WriteInteger(id);
                }
            }
        }
    }
}