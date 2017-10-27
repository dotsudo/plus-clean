﻿namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    using System;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using Outgoing.Rooms.Notifications;

    internal class PlaceObjectEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || !session.GetHabbo().InRoom)
            {
                return;
            }

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                return;
            }

            var rawData = packet.PopString();
            var data = rawData.Split(' ');

            if (!int.TryParse(data[0], out var itemId))
            {
                return;
            }

            var hasRights = room.CheckRights(session, false, true);

            if (!hasRights)
            {
                session.SendPacket(new RoomNotificationComposer("furni_placement_error", "message", "${room.error.cant_set_not_owner}"));
                return;
            }

            var item = session.GetHabbo().GetInventoryComponent().GetItem(itemId);

            if (item == null)
            {
                return;
            }

            if (room.GetRoomItemHandler().GetWallAndFloor.Count() > Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("room.item.placement_limit")))
            {
                session.SendNotification("You cannot have more than " + Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("room.item.placement_limit")) +
                                         " items in a room!");
                return;
            }

            if (item.Data.InteractionType == InteractionType.Exchange && room.OwnerId != session.GetHabbo().Id &&
                !session.GetHabbo().GetPermissions().HasRight("room_item_place_exchange_anywhere"))
            {
                session.SendNotification("You cannot place exchange items in other people's rooms!");
                return;
            }

            //TODO: Make neat.
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.Moodlight:
                {
                    var moodData = room.MoodlightData;
                    if (moodData != null && room.GetRoomItemHandler().GetItem(moodData.ItemId) != null)
                    {
                        session.SendNotification("You can only have one background moodlight per room!");
                        return;
                    }

                    break;
                }
                case InteractionType.Toner:
                {
                    var tonerData = room.TonerData;
                    if (tonerData != null && room.GetRoomItemHandler().GetItem(tonerData.ItemId) != null)
                    {
                        session.SendNotification("You can only have one background toner per room!");
                        return;
                    }

                    break;
                }
                case InteractionType.Hopper:
                {
                    if (room.GetRoomItemHandler().HopperCount > 0)
                    {
                        session.SendNotification("You can only have one hopper per room!");
                        return;
                    }

                    break;
                }

                case InteractionType.Tent:
                case InteractionType.TentSmall:
                {
                    room.AddTent(item.Id);
                    break;
                }
            }

            if (!item.IsWallItem)
            {
                if (data.Length < 4)
                {
                    return;
                }

                int y;

                if (!int.TryParse(data[1], out var x))
                {
                    return;
                }

                if (!int.TryParse(data[2], out y))
                {
                    return;
                }

                if (!int.TryParse(data[3], out var rotation))
                {
                    return;
                }

                var roomItem = new Item(item.Id, room.RoomId, item.BaseItem, item.ExtraData, x, y, 0, rotation, session.GetHabbo().Id, item.GroupId, item.LimitedNo,
                    item.LimitedTot, string.Empty, room);

                if (room.GetRoomItemHandler().SetFloorItem(session, roomItem, x, y, rotation, true, false, true))
                {
                    session.GetHabbo().GetInventoryComponent().RemoveItem(itemId);

                    if (session.GetHabbo().Id == room.OwnerId)
                    {
                        PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_RoomDecoFurniCount", 1);
                    }

                    if (!roomItem.IsWired)
                    {
                        return;
                    }

                    try
                    {
                        room.GetWired().LoadWiredBox(roomItem);
                    }
                    catch
                    {
                        Console.WriteLine(item.GetBaseItem().InteractionType);
                    }
                }
                else
                {
                    session.SendPacket(new RoomNotificationComposer("furni_placement_error", "message", "${room.error.cant_set_item}"));
                }
            }
            else if (item.IsWallItem)
            {
                var correctedData = new string[data.Length - 1];

                for (var i = 1; i < data.Length; i++)
                {
                    correctedData[i - 1] = data[i];
                }

                if (TrySetWallItem(correctedData, out var wallPos))
                {
                    try
                    {
                        var roomItem = new Item(item.Id, room.RoomId, item.BaseItem, item.ExtraData, 0, 0, 0, 0, session.GetHabbo().Id, item.GroupId, item.LimitedNo,
                            item.LimitedTot, wallPos, room);

                        if (room.GetRoomItemHandler().SetWallItem(session, roomItem))
                        {
                            session.GetHabbo().GetInventoryComponent().RemoveItem(itemId);
                            if (session.GetHabbo().Id == room.OwnerId)
                            {
                                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_RoomDecoFurniCount", 1);
                            }
                        }
                    }
                    catch
                    {
                        session.SendPacket(new RoomNotificationComposer("furni_placement_error", "message", "${room.error.cant_set_item}"));
                    }
                }
                else
                {
                    session.SendPacket(new RoomNotificationComposer("furni_placement_error", "message", "${room.error.cant_set_item}"));
                }
            }
        }

        private static bool TrySetWallItem(string[] data, out string position)
        {
            if (data.Length != 3 || !data[0].StartsWith(":w=") || !data[1].StartsWith("l=") || data[2] != "r" && data[2] != "l")
            {
                position = null;
                return false;
            }

            var wBit = data[0].Substring(3, data[0].Length - 3);
            var lBit = data[1].Substring(2, data[1].Length - 2);

            if (!wBit.Contains(",") || !lBit.Contains(","))
            {
                position = null;
                return false;
            }

            int.TryParse(wBit.Split(',')[0], out var w1);
            int.TryParse(wBit.Split(',')[1], out var w2);
            int.TryParse(lBit.Split(',')[0], out var l1);
            int.TryParse(lBit.Split(',')[1], out var l2);

            var wallPos = ":w=" + w1 + "," + w2 + " l=" + l1 + "," + l2 + " " + data[2];

            position = WallPositionCheck(wallPos);

            return position != null;
        }

        // @distributi0n TODO: Put this in a utility, it shouldn't be here.
        private static string WallPositionCheck(string wallPosition)
        {
            try
            {
                if (wallPosition.Contains(Convert.ToChar(13)))
                {
                    return null;
                }

                if (wallPosition.Contains(Convert.ToChar(9)))
                {
                    return null;
                }

                var posD = wallPosition.Split(' ');
                if (posD[2] != "l" && posD[2] != "r")
                {
                    return null;
                }

                var widD = posD[0].Substring(3).Split(',');
                var widthX = int.Parse(widD[0]);
                var widthY = int.Parse(widD[1]);

                if (widthX < -1000 || widthY < -1 || widthX > 700 || widthY > 700)
                {
                    return null;
                }

                var lenD = posD[1].Substring(2).Split(',');
                var lengthX = int.Parse(lenD[0]);
                var lengthY = int.Parse(lenD[1]);

                if (lengthX < -1 || lengthY < -1000 || lengthX > 700 || lengthY > 700)
                {
                    return null;
                }

                return ":w=" + widthX + "," + widthY + " " + "l=" + lengthX + "," + lengthY + " " + posD[2];
            }
            catch
            {
                return null;
            }
        }
    }
}