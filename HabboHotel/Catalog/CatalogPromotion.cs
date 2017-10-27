namespace Plus.HabboHotel.Catalog
{
    public class CatalogPromotion
    {
        public CatalogPromotion(int id, string title, string image, int unknown, string pageLink, int parentId)
        {
            Id = id;
            Title = title;
            Image = image;
            Unknown = unknown;
            PageLink = pageLink;
            ParentId = parentId;
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public int Unknown { get; set; }
        public string PageLink { get; set; }
        public int ParentId { get; set; }
    }
}