namespace Plus.HabboHotel.Catalog
{
    using System.Collections.Generic;

    public class CatalogPage
    {
        internal CatalogPage(int id,
                             int parentId,
                             string enabled,
                             string caption,
                             string pageLink,
                             int icon,
                             int minRank,
                             int minVip,
                             string visible,
                             string template,
                             string pageStrings1,
                             string pageStrings2,
                             Dictionary<int, CatalogItem> items,
                             ref Dictionary<int, int> flatOffers)
        {
            Id = id;
            ParentId = parentId;
            Enabled = enabled.ToLower() == "1" ? true : false;
            Caption = caption;
            PageLink = pageLink;
            Icon = icon;
            MinimumRank = minRank;
            MinimumVip = minVip;
            Visible = visible.ToLower() == "1" ? true : false;
            Template = template;
            foreach (var str in pageStrings1.Split('|'))
            {
                if (PageStrings1 == null)
                {
                    PageStrings1 = new List<string>();
                }
                PageStrings1.Add(str);
            }
            foreach (var str in pageStrings2.Split('|'))
            {
                if (PageStrings2 == null)
                {
                    PageStrings2 = new List<string>();
                }
                PageStrings2.Add(str);
            }

            Items = items;
            ItemOffers = new Dictionary<int, CatalogItem>();
            foreach (var i in flatOffers.Keys)
            {
                if (flatOffers[i] == id)
                {
                    foreach (var item in Items.Values)
                    {
                        if (item.OfferId == i)
                        {
                            if (!ItemOffers.ContainsKey(i))
                            {
                                ItemOffers.Add(i, item);
                            }
                        }
                    }
                }
            }
        }

        internal int Id { get; set; }

        internal int ParentId { get; set; }

        internal bool Enabled { get; set; }

        internal string Caption { get; set; }

        internal string PageLink { get; set; }

        internal int Icon { get; set; }

        internal int MinimumRank { get; set; }

        internal int MinimumVip { get; set; }

        internal bool Visible { get; set; }

        public string Template { get; set; }

        public List<string> PageStrings1 { get; }

        public List<string> PageStrings2 { get; }

        public Dictionary<int, CatalogItem> Items { get; }

        public Dictionary<int, CatalogItem> ItemOffers { get; }
    }
}