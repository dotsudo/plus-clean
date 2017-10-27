namespace Plus.Communication.Packets.Outgoing.Catalog
{
    using System.Collections.Generic;
    using HabboHotel.Catalog;
    using HabboHotel.GameClients;

    internal class CatalogIndexComposer : ServerPacket
    {
        internal CatalogIndexComposer(GameClient session, ICollection<CatalogPage> pages, int sub = 0)
            : base(ServerPacketHeader.CatalogIndexMessageComposer)
        {
            WriteRootIndex(session, pages);

            foreach (var parent in pages)
            {
                if (parent.ParentId != -1 || parent.MinimumRank > session.GetHabbo().Rank || parent.MinimumVip > session.GetHabbo().VipRank && session.GetHabbo().Rank == 1)
                {
                    continue;
                }

                WritePage(parent, CalcTreeSize(session, pages, parent.Id));

                foreach (var child in pages)
                {
                    if (child.ParentId != parent.Id || child.MinimumRank > session.GetHabbo().Rank || child.MinimumVip > session.GetHabbo().VipRank && session.GetHabbo().Rank == 1)
                    {
                        continue;
                    }

                    if (child.Enabled)
                    {
                        WritePage(child, CalcTreeSize(session, pages, child.Id));
                    }
                    else
                    {
                        WriteNodeIndex(child, CalcTreeSize(session, pages, child.Id));
                    }

                    foreach (var subChild in pages)
                    {
                        if (subChild.ParentId != child.Id || subChild.MinimumRank > session.GetHabbo().Rank)
                        {
                            continue;
                        }

                        WritePage(subChild, 0);
                    }
                }
            }

            WriteBoolean(false);
            WriteString("NORMAL");
        }

        public void WriteRootIndex(GameClient session, ICollection<CatalogPage> pages)
        {
            WriteBoolean(true);
            WriteInteger(0);
            WriteInteger(-1);
            WriteString("root");
            WriteString(string.Empty);
            WriteInteger(0);
            WriteInteger(CalcTreeSize(session, pages, -1));
        }

        public void WriteNodeIndex(CatalogPage page, int treeSize)
        {
            WriteBoolean(page.Visible);
            WriteInteger(page.Icon);
            WriteInteger(-1);
            WriteString(page.PageLink);
            WriteString(page.Caption);
            WriteInteger(0);
            WriteInteger(treeSize);
        }

        public void WritePage(CatalogPage page, int treeSize)
        {
            WriteBoolean(page.Visible);
            WriteInteger(page.Icon);
            WriteInteger(page.Id);
            WriteString(page.PageLink);
            WriteString(page.Caption);

            WriteInteger(page.ItemOffers.Count);
            foreach (var i in page.ItemOffers.Keys)
            {
                WriteInteger(i);
            }

            WriteInteger(treeSize);
        }

        public int CalcTreeSize(GameClient session, ICollection<CatalogPage> pages, int parentId)
        {
            var i = 0;
            foreach (var page in pages)
            {
                if (page.MinimumRank > session.GetHabbo().Rank || page.MinimumVip > session.GetHabbo().VipRank && session.GetHabbo().Rank == 1 || page.ParentId != parentId)
                {
                    continue;
                }

                if (page.ParentId == parentId)
                {
                    i++;
                }
            }

            return i;
        }
    }
}