namespace Plus.HabboHotel.Catalog
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Items;

    public class CatalogDeal
    {
        public CatalogDeal(int Id, string Items, string DisplayName, int RoomId, ItemDataManager ItemDataManager)
        {
            this.Id = Id;
            this.DisplayName = DisplayName;
            this.RoomId = RoomId;
            ItemDataList = new List<CatalogItem>();
            if (RoomId != 0)
            {
                DataTable getRoomItems = null;
                var roomItems = new Dictionary<int, int>();
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery(
                        "SELECT `items`.base_item, COALESCE(`items_groups`.`group_id`, 0) AS `group_id` FROM `items` LEFT OUTER JOIN `items_groups` ON `items`.`id` = `items_groups`.`id` WHERE `items`.`room_id` = @rid;");
                    dbClient.AddParameter("rid", RoomId);
                    getRoomItems = dbClient.GetTable();
                }
                if (getRoomItems != null)
                {
                    foreach (DataRow dRow in getRoomItems.Rows)
                    {
                        var item_id = Convert.ToInt32(dRow["base_item"]);
                        if (roomItems.ContainsKey(item_id))
                        {
                            roomItems[item_id]++;
                        }
                        else
                        {
                            roomItems.Add(item_id, 1);
                        }
                    }
                }

                foreach (var roomItem in roomItems)
                {
                    Items += roomItem.Key + "*" + roomItem.Value + ";";
                }

                if (roomItems.Count > 0)
                {
                    Items = Items.Remove(Items.Length - 1);
                }
            }

            var SplitItems = Items.Split(';');
            foreach (var Split in SplitItems)
            {
                var Item = Split.Split('*');
                var ItemId = 0;
                var Amount = 0;
                if (!int.TryParse(Item[0], out ItemId) || !int.TryParse(Item[1], out Amount))
                {
                    continue;
                }

                ItemData Data = null;
                if (!ItemDataManager.GetItem(ItemId, out Data))
                {
                    continue;
                }

                ItemDataList.Add(new CatalogItem(0, ItemId, Data, string.Empty, 0, 0, 0, 0, Amount, 0, 0, false, "", "", 0));
            }
        }

        public int Id { get; set; }
        public List<CatalogItem> ItemDataList { get; }
        public string DisplayName { get; set; }
        public int RoomId { get; set; }
    }
}