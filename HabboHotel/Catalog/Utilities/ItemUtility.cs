namespace Plus.HabboHotel.Catalog.Utilities
{
    using Items;

    public static class ItemUtility
    {
        public static bool CanGiftItem(CatalogItem Item)
        {
            if (!Item.Data.AllowGift ||
                Item.IsLimited ||
                Item.Amount > 1 ||
                Item.Data.InteractionType == InteractionType.Exchange ||
                Item.Data.InteractionType == InteractionType.Badge ||
                Item.Data.Type != 's' && Item.Data.Type != 'i' ||
                Item.CostDiamonds > 0 ||
                Item.Data.InteractionType == InteractionType.Teleport ||
                Item.Data.InteractionType == InteractionType.Deal)
            {
                return false;
            }
            if (Item.Data.IsRare)
            {
                return false;
            }
            if (Item.Data.InteractionType == InteractionType.Pet)
            {
                return false;
            }

            return true;
        }

        public static bool CanSelectAmount(CatalogItem Item)
        {
            if (Item.IsLimited ||
                Item.Amount > 1 ||
                Item.Data.InteractionType == InteractionType.Exchange ||
                !Item.HaveOffer ||
                Item.Data.InteractionType == InteractionType.Badge ||
                Item.Data.InteractionType == InteractionType.Deal)
            {
                return false;
            }

            return true;
        }

        public static int GetSaddleId(int Saddle)
        {
            switch (Saddle)
            {
                default:
                case 9:
                    return 4221;
                case 10:
                    return 4450;
            }
        }

        public static bool IsRare(Item Item)
        {
            if (Item.LimitedNo > 0)
            {
                return true;
            }
            if (Item.Data.IsRare)
            {
                return true;
            }

            return false;
        }
    }
}