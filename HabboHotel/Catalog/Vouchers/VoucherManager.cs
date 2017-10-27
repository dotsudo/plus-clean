namespace Plus.HabboHotel.Catalog.Vouchers
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    public class VoucherManager
    {
        private readonly Dictionary<string, Voucher> _vouchers;

        public VoucherManager() => _vouchers = new Dictionary<string, Voucher>();

        public void Init()
        {
            if (_vouchers.Count > 0)
            {
                _vouchers.Clear();
            }
            DataTable GetVouchers = null;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `voucher`,`type`,`value`,`current_uses`,`max_uses` FROM `catalog_vouchers` WHERE `enabled` = '1'");
                GetVouchers = dbClient.GetTable();
            }
            if (GetVouchers != null)
            {
                foreach (DataRow Row in GetVouchers.Rows)
                {
                    _vouchers.Add(Convert.ToString(Row["voucher"]),
                        new Voucher(Convert.ToString(Row["voucher"]),
                            Convert.ToString(Row["type"]),
                            Convert.ToInt32(Row["value"]),
                            Convert.ToInt32(Row["current_uses"]),
                            Convert.ToInt32(Row["max_uses"])));
                }
            }
        }

        public bool TryGetVoucher(string Code, out Voucher Voucher)
        {
            if (_vouchers.TryGetValue(Code, out Voucher))
            {
                return true;
            }

            return false;
        }
    }
}