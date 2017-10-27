﻿namespace Plus.HabboHotel.Rooms.Trading
{
    using System.Collections.Generic;
    using Items;

    public sealed class TradeUser
    {
        public TradeUser(RoomUser User)
        {
            RoomUser = User;
            HasAccepted = false;
            OfferedItems = new Dictionary<int, Item>();
        }

        public RoomUser RoomUser { get; }

        public bool HasAccepted { get; set; }

        public Dictionary<int, Item> OfferedItems { get; set; }
    }
}