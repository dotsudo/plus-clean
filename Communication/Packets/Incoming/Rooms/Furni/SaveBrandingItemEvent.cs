﻿namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    using System;
    using HabboHotel.GameClients;
    using HabboHotel.Items;

    internal class SaveBrandingItemEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().InRoom)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            if (!room.CheckRights(session, true) || !session.GetHabbo().GetPermissions().HasRight("room_item_save_branding_items"))
            {
                return;
            }

            var itemId = packet.PopInt();
            var item = room.GetRoomItemHandler().GetItem(itemId);
            if (item == null)
            {
                return;
            }

            if (item.Data.InteractionType == InteractionType.Background)
            {
                var data = packet.PopInt();
                var brandData = "state" + Convert.ToChar(9) + "0";

                for (var i = 1; i <= data; i++)
                {
                    brandData = brandData + Convert.ToChar(9) + packet.PopString();
                }

                item.ExtraData = brandData;
            }
            else if (item.Data.InteractionType == InteractionType.FxProvider)
            {
                /*int Unknown = Packet.PopInt();
                string Data = Packet.PopString();
                int EffectId = Packet.PopInt();

                Item.ExtraData = Convert.ToString(EffectId);*/
            }

            room.GetRoomItemHandler().SetFloorItem(session, item, item.GetX, item.GetY, item.Rotation, false, false, true);
        }
    }
}