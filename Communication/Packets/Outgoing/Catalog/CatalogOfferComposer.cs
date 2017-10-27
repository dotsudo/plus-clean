namespace Plus.Communication.Packets.Outgoing.Catalog
{
    using HabboHotel.Catalog;
    using HabboHotel.Catalog.Utilities;
    using HabboHotel.Items;

    internal class CatalogOfferComposer : ServerPacket
    {
        public CatalogOfferComposer(CatalogItem item)
            : base(ServerPacketHeader.CatalogOfferMessageComposer)
        {
            WriteInteger(item.OfferId);
            WriteString(item.Data.ItemName);
            WriteBoolean(false); //IsRentable
            WriteInteger(item.CostCredits);

            if (item.CostDiamonds > 0)
            {
                WriteInteger(item.CostDiamonds);
                WriteInteger(5); // Diamonds
            }
            else
            {
                WriteInteger(item.CostPixels);
                WriteInteger(0); // Type of PixelCost
            }

            WriteBoolean(ItemUtility.CanGiftItem(item));
            WriteInteger(string.IsNullOrEmpty(item.Badge) ? 1 : 2); //Count 1 item if there is no badge, otherwise count as 2.

            if (!string.IsNullOrEmpty(item.Badge))
            {
                WriteString("b");
                WriteString(item.Badge);
            }

            WriteString(item.Data.Type.ToString());
            if (item.Data.Type.ToString().ToLower() == "b")
            {
                WriteString(item.Data.ItemName); //Badge name.
            }
            else
            {
                WriteInteger(item.Data.SpriteId);
                if (item.Data.InteractionType == InteractionType.Wallpaper || item.Data.InteractionType == InteractionType.Floor ||
                    item.Data.InteractionType == InteractionType.Landscape)
                {
                    WriteString(item.Name.Split('_')[2]);
                }
                else if (item.PageID == 9) //Bots
                {
                    WriteString(!PlusEnvironment.GetGame().GetCatalog().TryGetBot(item.ItemId, out var cataBot)
                        ? "hd-180-7.ea-1406-62.ch-210-1321.hr-831-49.ca-1813-62.sh-295-1321.lg-285-92"
                        : cataBot.Figure);
                }
                else if (item.ExtraData != null)
                {
                    WriteString(item.ExtraData != null ? item.ExtraData : string.Empty);
                }
                WriteInteger(item.Amount);
                WriteBoolean(item.IsLimited); // IsLimited
                if (item.IsLimited)
                {
                    WriteInteger(item.LimitedEditionStack);
                    WriteInteger(item.LimitedEditionStack - item.LimitedEditionSells);
                }
            }
            WriteInteger(0); // club_level
            WriteBoolean(ItemUtility.CanSelectAmount(item));
            WriteBoolean(false); // TODO: Figure out
            WriteString(""); //previewImage -> e.g; catalogue/pet_lion.png
        }
    }
}