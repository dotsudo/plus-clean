namespace Plus.HabboHotel.Items
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Rooms;

    public static class ItemLoader
    {
        public static List<Item> GetItemsForRoom(int roomId, Room room)
        {
            DataTable items = null;
            var I = new List<Item>();
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `items`.*, COALESCE(`items_groups`.`group_id`, 0) AS `group_id` FROM `items` LEFT OUTER JOIN `items_groups` ON `items`.`id` = `items_groups`.`id` WHERE `items`.`room_id` = @rid;");
                dbClient.AddParameter("rid", roomId);
                items = dbClient.GetTable();
                if (items != null)
                {
                    foreach (DataRow row in items.Rows)
                    {
                        ItemData data = null;
                        if (PlusEnvironment.GetGame().GetItemManager().GetItem(Convert.ToInt32(row["base_item"]), out data))
                        {
                            I.Add(new Item(Convert.ToInt32(row["id"]),
                                Convert.ToInt32(row["room_id"]),
                                Convert.ToInt32(row["base_item"]),
                                Convert.ToString(row["extra_data"]),
                                Convert.ToInt32(row["x"]),
                                Convert.ToInt32(row["y"]),
                                Convert.ToDouble(row["z"]),
                                Convert.ToInt32(row["rot"]),
                                Convert.ToInt32(row["user_id"]),
                                Convert.ToInt32(row["group_id"]),
                                Convert.ToInt32(row["limited_number"]),
                                Convert.ToInt32(row["limited_stack"]),
                                Convert.ToString(row["wall_pos"]),
                                room));
                        }
                    }
                }
            }

            return I;
        }

        public static List<Item> GetItemsForUser(int userId)
        {
            DataTable items = null;
            var I = new List<Item>();
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `items`.*, COALESCE(`items_groups`.`group_id`, 0) AS `group_id` FROM `items` LEFT OUTER JOIN `items_groups` ON `items`.`id` = `items_groups`.`id` WHERE `items`.`room_id` = 0 AND `items`.`user_id` = @uid;");
                dbClient.AddParameter("uid", userId);
                items = dbClient.GetTable();
                if (items != null)
                {
                    foreach (DataRow row in items.Rows)
                    {
                        ItemData data = null;
                        if (PlusEnvironment.GetGame().GetItemManager().GetItem(Convert.ToInt32(row["base_item"]), out data))
                        {
                            I.Add(new Item(Convert.ToInt32(row["id"]),
                                Convert.ToInt32(row["room_id"]),
                                Convert.ToInt32(row["base_item"]),
                                Convert.ToString(row["extra_data"]),
                                Convert.ToInt32(row["x"]),
                                Convert.ToInt32(row["y"]),
                                Convert.ToDouble(row["z"]),
                                Convert.ToInt32(row["rot"]),
                                Convert.ToInt32(row["user_id"]),
                                Convert.ToInt32(row["group_id"]),
                                Convert.ToInt32(row["limited_number"]),
                                Convert.ToInt32(row["limited_stack"]),
                                Convert.ToString(row["wall_pos"])));
                        }
                    }
                }
            }

            return I;
        }
    }
}