namespace Plus.HabboHotel.Catalog.Vouchers
{
    public class Voucher
    {
        internal Voucher(string code, string type, int value, int currentUses, int maxUses)
        {
            Code = code;
            Type = VoucherUtility.GetType(type);
            Value = value;
            CurrentUses = currentUses;
            MaxUses = maxUses;
        }

        private string Code { get; }
        internal VoucherType Type { get; }
        internal int Value { get; }
        internal int CurrentUses { get; private set; }
        internal int MaxUses { get; }

        internal void UpdateUses()
        {
            CurrentUses += 1;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `catalog_vouchers` SET `current_uses` = `current_uses` + '1' WHERE `voucher` = '" +
                                  Code + "' LIMIT 1");
            }
        }
    }
}