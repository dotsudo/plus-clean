namespace Plus.HabboHotel.Items
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using log4net;

    public class ItemDataManager
    {
        private static readonly ILog Log = LogManager.GetLogger("Plus.HabboHotel.Items.ItemDataManager");
        public Dictionary<int, ItemData> Gifts; //<SpriteId, Item>

        public Dictionary<int, ItemData> Items;

        public ItemDataManager()
        {
            Items = new Dictionary<int, ItemData>();
            Gifts = new Dictionary<int, ItemData>();
        }

        public void Init()
        {
            if (Items.Count > 0)
            {
                Items.Clear();
            }
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `furniture`");
                var itemData = dbClient.GetTable();
                if (itemData != null)
                {
                    foreach (DataRow row in itemData.Rows)
                    {
                        try
                        {
                            var id = Convert.ToInt32(row["id"]);
                            var spriteId = Convert.ToInt32(row["sprite_id"]);
                            var itemName = Convert.ToString(row["item_name"]);
                            var publicName = Convert.ToString(row["public_name"]);
                            var type = row["type"].ToString();
                            var width = Convert.ToInt32(row["width"]);
                            var length = Convert.ToInt32(row["length"]);
                            var height = Convert.ToDouble(row["stack_height"]);
                            var allowStack = PlusEnvironment.EnumToBool(row["can_stack"].ToString());
                            var allowWalk = PlusEnvironment.EnumToBool(row["is_walkable"].ToString());
                            var allowSit = PlusEnvironment.EnumToBool(row["can_sit"].ToString());
                            var allowRecycle = PlusEnvironment.EnumToBool(row["allow_recycle"].ToString());
                            var allowTrade = PlusEnvironment.EnumToBool(row["allow_trade"].ToString());
                            var allowMarketplace = Convert.ToInt32(row["allow_marketplace_sell"]) == 1;
                            var allowGift = Convert.ToInt32(row["allow_gift"]) == 1;
                            var allowInventoryStack = PlusEnvironment.EnumToBool(row["allow_inventory_stack"].ToString());
                            var interactionType = InteractionTypes.GetTypeFromString(Convert.ToString(row["interaction_type"]));
                            var behaviourData = Convert.ToInt32(row["behaviour_data"]);
                            var cycleCount = Convert.ToInt32(row["interaction_modes_count"]);
                            var vendingIds = Convert.ToString(row["vending_ids"]);
                            var heightAdjustable = Convert.ToString(row["height_adjustable"]);
                            var effectId = Convert.ToInt32(row["effect_id"]);
                            var isRare = PlusEnvironment.EnumToBool(row["is_rare"].ToString());
                            var extraRot = PlusEnvironment.EnumToBool(row["extra_rot"].ToString());
                            if (!Gifts.ContainsKey(spriteId))
                            {
                                Gifts.Add(spriteId,
                                    new ItemData(id,
                                        spriteId,
                                        itemName,
                                        publicName,
                                        type,
                                        width,
                                        length,
                                        height,
                                        allowStack,
                                        allowWalk,
                                        allowSit,
                                        allowRecycle,
                                        allowTrade,
                                        allowMarketplace,
                                        allowGift,
                                        allowInventoryStack,
                                        interactionType,
                                        behaviourData,
                                        cycleCount,
                                        vendingIds,
                                        heightAdjustable,
                                        effectId,
                                        isRare,
                                        extraRot));
                            }
                            if (!Items.ContainsKey(id))
                            {
                                Items.Add(id,
                                    new ItemData(id,
                                        spriteId,
                                        itemName,
                                        publicName,
                                        type,
                                        width,
                                        length,
                                        height,
                                        allowStack,
                                        allowWalk,
                                        allowSit,
                                        allowRecycle,
                                        allowTrade,
                                        allowMarketplace,
                                        allowGift,
                                        allowInventoryStack,
                                        interactionType,
                                        behaviourData,
                                        cycleCount,
                                        vendingIds,
                                        heightAdjustable,
                                        effectId,
                                        isRare,
                                        extraRot));
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            Console.ReadKey();

                            //Logging.WriteLine("Could not load item #" + Convert.ToInt32(Row[0]) + ", please verify the data is okay.");
                        }
                    }
                }
            }

            Log.Info("Item Manager -> LOADED");
        }

        public bool GetItem(int id, out ItemData item)
        {
            if (Items.TryGetValue(id, out item))
            {
                return true;
            }

            return false;
        }

        public ItemData GetItemByName(string name)
        {
            foreach (var entry in Items)
            {
                var item = entry.Value;
                if (item.ItemName == name)
                {
                    return item;
                }
            }

            return null;
        }

        public bool GetGift(int spriteId, out ItemData item)
        {
            if (Gifts.TryGetValue(spriteId, out item))
            {
                return true;
            }

            return false;
        }
    }
}