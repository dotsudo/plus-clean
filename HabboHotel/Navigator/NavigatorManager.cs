﻿namespace Plus.HabboHotel.Navigator
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using log4net;

    public sealed class NavigatorManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Navigator.NavigatorManager");

        private readonly Dictionary<int, FeaturedRoom> _featuredRooms;
        private readonly Dictionary<int, SearchResultList> _searchResultLists;

        private readonly Dictionary<int, TopLevelItem> _topLevelItems;

        internal NavigatorManager()
        {
            _topLevelItems = new Dictionary<int, TopLevelItem>();
            _searchResultLists = new Dictionary<int, SearchResultList>();

            //Does this need to be dynamic?
            _topLevelItems.Add(1, new TopLevelItem(1, "official_view", "", ""));
            _topLevelItems.Add(2, new TopLevelItem(2, "hotel_view", "", ""));
            _topLevelItems.Add(3, new TopLevelItem(3, "roomads_view", "", ""));
            _topLevelItems.Add(4, new TopLevelItem(4, "myworld_view", "", ""));
            _featuredRooms = new Dictionary<int, FeaturedRoom>();
            Init();
        }

        internal void Init()
        {
            if (_searchResultLists.Count > 0)
            {
                _searchResultLists.Clear();
            }
            if (_featuredRooms.Count > 0)
            {
                _featuredRooms.Clear();
            }
            DataTable Table = null;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `navigator_categories` ORDER BY `id` ASC");
                Table = dbClient.GetTable();
                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        if (Convert.ToInt32(Row["enabled"]) == 1)
                        {
                            if (!_searchResultLists.ContainsKey(Convert.ToInt32(Row["id"])))
                            {
                                _searchResultLists.Add(Convert.ToInt32(Row["id"]),
                                    new SearchResultList(Convert.ToInt32(Row["id"]),
                                        Convert.ToString(Row["category"]),
                                        Convert.ToString(Row["category_identifier"]),
                                        Convert.ToString(Row["public_name"]),
                                        true,
                                        -1,
                                        Convert.ToInt32(Row["required_rank"]),
                                        NavigatorViewModeUtility.GetViewModeByString(Convert.ToString(Row["view_mode"])),
                                        Convert.ToString(Row["category_type"]),
                                        Convert.ToString(Row["search_allowance"]),
                                        Convert.ToInt32(Row["order_id"])));
                            }
                        }
                    }
                }

                dbClient.SetQuery(
                    "SELECT `room_id`,`caption`,`description`,`image_url`,`enabled` FROM `navigator_publics` ORDER BY `order_num` ASC");
                var GetPublics = dbClient.GetTable();
                if (GetPublics != null)
                {
                    foreach (DataRow Row in GetPublics.Rows)
                    {
                        if (Convert.ToInt32(Row["enabled"]) == 1)
                        {
                            if (!_featuredRooms.ContainsKey(Convert.ToInt32(Row["room_id"])))
                            {
                                _featuredRooms.Add(Convert.ToInt32(Row["room_id"]),
                                    new FeaturedRoom(Convert.ToInt32(Row["room_id"]),
                                        Convert.ToString(Row["caption"]),
                                        Convert.ToString(Row["description"]),
                                        Convert.ToString(Row["image_url"])));
                            }
                        }
                    }
                }
            }

            log.Info("Navigator -> LOADED");
        }

        public List<SearchResultList> GetCategorysForSearch(string Category)
        {
            var Categorys = from Cat in _searchResultLists
                where Cat.Value.Category == Category
                orderby Cat.Value.OrderId
                select Cat.Value;
            return Categorys.ToList();
        }

        public ICollection<SearchResultList> GetResultByIdentifier(string Category)
        {
            var Categorys = from Cat in _searchResultLists
                where Cat.Value.CategoryIdentifier == Category
                orderby Cat.Value.OrderId
                select Cat.Value;
            return Categorys.ToList();
        }

        public ICollection<SearchResultList> GetFlatCategories()
        {
            var Categorys = from Cat in _searchResultLists
                where Cat.Value.CategoryType == NavigatorCategoryType.CATEGORY
                orderby Cat.Value.OrderId
                select Cat.Value;
            return Categorys.ToList();
        }

        public ICollection<SearchResultList> GetEventCategories()
        {
            var Categorys = from Cat in _searchResultLists
                where Cat.Value.CategoryType == NavigatorCategoryType.PROMOTION_CATEGORY
                orderby Cat.Value.OrderId
                select Cat.Value;
            return Categorys.ToList();
        }

        public ICollection<TopLevelItem> GetTopLevelItems() => _topLevelItems.Values;

        public ICollection<SearchResultList> GetSearchResultLists() => _searchResultLists.Values;

        public bool TryGetTopLevelItem(int Id, out TopLevelItem TopLevelItem) => _topLevelItems.TryGetValue(Id, out TopLevelItem);

        public bool TryGetSearchResultList(int Id, out SearchResultList SearchResultList) =>
            _searchResultLists.TryGetValue(Id, out SearchResultList);

        public bool TryGetFeaturedRoom(int RoomId, out FeaturedRoom PublicRoom) =>
            _featuredRooms.TryGetValue(RoomId, out PublicRoom);

        public ICollection<FeaturedRoom> GetFeaturedRooms() => _featuredRooms.Values;
    }
}