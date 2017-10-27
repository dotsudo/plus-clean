namespace Plus.HabboHotel.Users.Navigator.SavedSearches
{
    using System;
    using System.Collections.Concurrent;
    using System.Data;

    public class SearchesComponent
    {
        private readonly ConcurrentDictionary<int, SavedSearch> _savedSearches;

        internal SearchesComponent() => _savedSearches = new ConcurrentDictionary<int, SavedSearch>();

        internal bool Init(Habbo player)
        {
            if (_savedSearches.Count > 0)
            {
                _savedSearches.Clear();
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`filter`,`search_code` FROM `user_saved_searches` WHERE `user_id` = @UserId");
                dbClient.AddParameter("UserId", player.Id);
                var getSearches = dbClient.GetTable();
                if (getSearches != null)
                {
                    foreach (DataRow row in getSearches.Rows)
                    {
                        _savedSearches.TryAdd(Convert.ToInt32(row["id"]),
                            new SavedSearch(Convert.ToInt32(row["id"]), Convert.ToString(row["filter"]),
                                Convert.ToString(row["search_code"])));
                    }
                }
            }

            return true;
        }
    }
}