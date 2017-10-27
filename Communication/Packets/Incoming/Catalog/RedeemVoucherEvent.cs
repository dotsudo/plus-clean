namespace Plus.Communication.Packets.Incoming.Catalog
{
    using System.Data;
    using HabboHotel.Catalog.Vouchers;
    using HabboHotel.GameClients;
    using Outgoing.Catalog;
    using Outgoing.Inventory.Purse;

    internal class RedeemVoucherEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var voucherCode = packet.PopString().Replace("\r", "");

            if (!PlusEnvironment.GetGame().GetCatalog().GetVoucherManager().TryGetVoucher(voucherCode, out var voucher))
            {
                session.SendPacket(new VoucherRedeemErrorComposer(0));
                return;
            }

            if (voucher.CurrentUses >= voucher.MaxUses)
            {
                session.SendNotification("Oops, this voucher has reached the maximum usage limit!");
                return;
            }

            DataRow getRow;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `user_vouchers` WHERE `user_id` = @userId AND `voucher` = @Voucher LIMIT 1");
                dbClient.AddParameter("userId", session.GetHabbo().Id);
                dbClient.AddParameter("Voucher", voucherCode);
                getRow = dbClient.GetRow();
            }

            if (getRow != null)
            {
                session.SendNotification("You've already used this voucher code, one per each user, sorry!");
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `user_vouchers` (`user_id`,`voucher`) VALUES (@userId, @Voucher)");
                dbClient.AddParameter("userId", session.GetHabbo().Id);
                dbClient.AddParameter("Voucher", voucherCode);
                dbClient.RunQuery();
            }

            voucher.UpdateUses();

            if (voucher.Type == VoucherType.CREDIT)
            {
                session.GetHabbo().Credits += voucher.Value;
                session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
            }
            else if (voucher.Type == VoucherType.DUCKET)
            {
                session.GetHabbo().Duckets += voucher.Value;
                session.SendPacket(new HabboActivityPointNotificationComposer(session.GetHabbo().Duckets, voucher.Value));
            }

            session.SendPacket(new VoucherRedeemOkComposer());
        }
    }
}