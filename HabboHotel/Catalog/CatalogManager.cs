namespace Plus.HabboHotel.Catalog
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Clothing;
    using Items;
    using log4net;
    using Marketplace;
    using Pets;
    using Vouchers;

    public class CatalogManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Catalog.CatalogManager");
        private readonly Dictionary<int, CatalogBot> _botPresets;
        private readonly ClothingManager _clothingManager;
        private readonly Dictionary<int, CatalogDeal> _deals;
        private readonly Dictionary<int, Dictionary<int, CatalogItem>> _items;

        private readonly MarketplaceManager _marketplace;
        private readonly Dictionary<int, CatalogPage> _pages;
        private readonly PetRaceManager _petRaceManager;
        private readonly Dictionary<int, CatalogPromotion> _promotions;
        private readonly VoucherManager _voucherManager;

        private Dictionary<int, int> _itemOffers;

        public CatalogManager()
        {
            _marketplace = new MarketplaceManager();
            _petRaceManager = new PetRaceManager();
            _voucherManager = new VoucherManager();
            _voucherManager.Init();
            _clothingManager = new ClothingManager();
            _clothingManager.Init();
            _itemOffers = new Dictionary<int, int>();
            _pages = new Dictionary<int, CatalogPage>();
            _botPresets = new Dictionary<int, CatalogBot>();
            _items = new Dictionary<int, Dictionary<int, CatalogItem>>();
            _deals = new Dictionary<int, CatalogDeal>();
            _promotions = new Dictionary<int, CatalogPromotion>();
        }

        public Dictionary<int, int> ItemOffers => _itemOffers;

        public void Init(ItemDataManager ItemDataManager)
        {
            if (_pages.Count > 0)
            {
                _pages.Clear();
            }
            if (_botPresets.Count > 0)
            {
                _botPresets.Clear();
            }
            if (_items.Count > 0)
            {
                _items.Clear();
            }
            if (_deals.Count > 0)
            {
                _deals.Clear();
            }
            if (_promotions.Count > 0)
            {
                _promotions.Clear();
            }
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `id`,`item_id`,`catalog_name`,`cost_credits`,`cost_pixels`,`cost_diamonds`,`amount`,`page_id`,`limited_sells`,`limited_stack`,`offer_active`,`extradata`,`badge`,`offer_id` FROM `catalog_items`");
                var CatalogueItems = dbClient.GetTable();
                if (CatalogueItems != null)
                {
                    foreach (DataRow Row in CatalogueItems.Rows)
                    {
                        if (Convert.ToInt32(Row["amount"]) <= 0)
                        {
                            continue;
                        }

                        var ItemId = Convert.ToInt32(Row["id"]);
                        var PageId = Convert.ToInt32(Row["page_id"]);
                        var BaseId = Convert.ToInt32(Row["item_id"]);
                        var OfferId = Convert.ToInt32(Row["offer_id"]);
                        ItemData Data = null;
                        if (!ItemDataManager.GetItem(BaseId, out Data))
                        {
                            log.Error("Couldn't load Catalog Item " + ItemId + ", no furniture record found.");
                            continue;
                        }

                        if (!_items.ContainsKey(PageId))
                        {
                            _items[PageId] = new Dictionary<int, CatalogItem>();
                        }
                        if (OfferId != -1 && !_itemOffers.ContainsKey(OfferId))
                        {
                            _itemOffers.Add(OfferId, PageId);
                        }
                        _items[PageId]
                            .Add(Convert.ToInt32(Row["id"]),
                                new CatalogItem(Convert.ToInt32(Row["id"]),
                                    Convert.ToInt32(Row["item_id"]),
                                    Data,
                                    Convert.ToString(Row["catalog_name"]),
                                    Convert.ToInt32(Row["page_id"]),
                                    Convert.ToInt32(Row["cost_credits"]),
                                    Convert.ToInt32(Row["cost_pixels"]),
                                    Convert.ToInt32(Row["cost_diamonds"]),
                                    Convert.ToInt32(Row["amount"]),
                                    Convert.ToInt32(Row["limited_sells"]),
                                    Convert.ToInt32(Row["limited_stack"]),
                                    PlusEnvironment.EnumToBool(Row["offer_active"].ToString()),
                                    Convert.ToString(Row["extradata"]),
                                    Convert.ToString(Row["badge"]),
                                    Convert.ToInt32(Row["offer_id"])));
                    }
                }

                dbClient.SetQuery("SELECT `id`, `items`, `name`, `room_id` FROM `catalog_deals`");
                var GetDeals = dbClient.GetTable();
                if (GetDeals != null)
                {
                    foreach (DataRow Row in GetDeals.Rows)
                    {
                        var Id = Convert.ToInt32(Row["id"]);
                        var Items = Convert.ToString(Row["items"]);
                        var Name = Convert.ToString(Row["name"]);
                        var RoomId = Convert.ToInt32(Row["room_id"]);
                        var Deal = new CatalogDeal(Id, Items, Name, RoomId, ItemDataManager);
                        if (!_deals.ContainsKey(Id))
                        {
                            _deals.Add(Deal.Id, Deal);
                        }
                    }
                }

                dbClient.SetQuery(
                    "SELECT `id`,`parent_id`,`caption`,`page_link`,`visible`,`enabled`,`min_rank`,`min_vip`,`icon_image`,`page_layout`,`page_strings_1`,`page_strings_2` FROM `catalog_pages` ORDER BY `order_num`");
                var CatalogPages = dbClient.GetTable();
                if (CatalogPages != null)
                {
                    foreach (DataRow Row in CatalogPages.Rows)
                    {
                        _pages.Add(Convert.ToInt32(Row["id"]),
                            new CatalogPage(Convert.ToInt32(Row["id"]),
                                Convert.ToInt32(Row["parent_id"]),
                                Row["enabled"].ToString(),
                                Convert.ToString(Row["caption"]),
                                Convert.ToString(Row["page_link"]),
                                Convert.ToInt32(Row["icon_image"]),
                                Convert.ToInt32(Row["min_rank"]),
                                Convert.ToInt32(Row["min_vip"]),
                                Row["visible"].ToString(),
                                Convert.ToString(Row["page_layout"]),
                                Convert.ToString(Row["page_strings_1"]),
                                Convert.ToString(Row["page_strings_2"]),
                                _items.ContainsKey(Convert.ToInt32(Row["id"]))
                                    ? _items[Convert.ToInt32(Row["id"])]
                                    : new Dictionary<int, CatalogItem>(),
                                ref _itemOffers));
                    }
                }

                dbClient.SetQuery("SELECT `id`,`name`,`figure`,`motto`,`gender`,`ai_type` FROM `catalog_bot_presets`");
                var bots = dbClient.GetTable();
                if (bots != null)
                {
                    foreach (DataRow Row in bots.Rows)
                    {
                        _botPresets.Add(Convert.ToInt32(Row[0]),
                            new CatalogBot(Convert.ToInt32(Row[0]),
                                Convert.ToString(Row[1]),
                                Convert.ToString(Row[2]),
                                Convert.ToString(Row[3]),
                                Convert.ToString(Row[4]),
                                Convert.ToString(Row[5])));
                    }
                }

                dbClient.SetQuery("SELECT * FROM `catalog_promotions`");
                var GetPromotions = dbClient.GetTable();
                if (GetPromotions != null)
                {
                    foreach (DataRow Row in GetPromotions.Rows)
                    {
                        if (!_promotions.ContainsKey(Convert.ToInt32(Row["id"])))
                        {
                            _promotions.Add(Convert.ToInt32(Row["id"]),
                                new CatalogPromotion(Convert.ToInt32(Row["id"]),
                                    Convert.ToString(Row["title"]),
                                    Convert.ToString(Row["image"]),
                                    Convert.ToInt32(Row["unknown"]),
                                    Convert.ToString(Row["page_link"]),
                                    Convert.ToInt32(Row["parent_id"])));
                        }
                    }
                }

                _petRaceManager.Init();
                _clothingManager.Init();
            }

            log.Info("Catalog Manager -> LOADED");
        }

        public bool TryGetBot(int ItemId, out CatalogBot Bot) => _botPresets.TryGetValue(ItemId, out Bot);

        public bool TryGetPage(int pageId, out CatalogPage page) => _pages.TryGetValue(pageId, out page);

        public bool TryGetDeal(int dealId, out CatalogDeal deal) => _deals.TryGetValue(dealId, out deal);

        public ICollection<CatalogPage> GetPages() => _pages.Values;

        public ICollection<CatalogPromotion> GetPromotions() => _promotions.Values;

        public MarketplaceManager GetMarketplace() => _marketplace;

        public PetRaceManager GetPetRaceManager() => _petRaceManager;

        public VoucherManager GetVoucherManager() => _voucherManager;

        public ClothingManager GetClothingManager() => _clothingManager;
    }
}