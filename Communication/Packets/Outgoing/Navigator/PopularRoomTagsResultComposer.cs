﻿namespace Plus.Communication.Packets.Outgoing.Navigator
{
    using System.Collections.Generic;

    internal class PopularRoomTagsResultComposer : ServerPacket
    {
        public PopularRoomTagsResultComposer(ICollection<KeyValuePair<string, int>> tags)
            : base(ServerPacketHeader.PopularRoomTagsResultMessageComposer)
        {
            WriteInteger(tags.Count);
            foreach (var tag in tags)
            {
                WriteString(tag.Key);
                WriteInteger(tag.Value);
            }
        }
    }
}