﻿namespace Plus.HabboHotel.Items.Televisions
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using log4net;

    public class TelevisionManager
    {
        private static readonly ILog Log = LogManager.GetLogger("Plus.HabboHotel.Items.Televisions.TelevisionManager");

        public Dictionary<int, TelevisionItem> Televisions;

        public TelevisionManager()
        {
            Televisions = new Dictionary<int, TelevisionItem>();
            Init();
        }

        public ICollection<TelevisionItem> TelevisionList => Televisions.Values;

        public void Init()
        {
            if (Televisions.Count > 0)
            {
                Televisions.Clear();
            }
            DataTable getData = null;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `items_youtube` ORDER BY `id` DESC");
                getData = dbClient.GetTable();
                if (getData != null)
                {
                    foreach (DataRow row in getData.Rows)
                    {
                        Televisions.Add(Convert.ToInt32(row["id"]),
                            new TelevisionItem(Convert.ToInt32(row["id"]),
                                row["youtube_id"].ToString(),
                                row["title"].ToString(),
                                row["description"].ToString(),
                                PlusEnvironment.EnumToBool(row["enabled"].ToString())));
                    }
                }
            }

            Log.Info("Television Items -> LOADED");
        }

        public bool TryGet(int itemId, out TelevisionItem televisionItem)
        {
            if (Televisions.TryGetValue(itemId, out televisionItem))
            {
                return true;
            }

            return false;
        }
    }
}