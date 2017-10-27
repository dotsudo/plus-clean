namespace Plus.HabboHotel.Cache
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using log4net;
    using Process;
    using Type;

    public class CacheManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Cache.CacheManager");
        private readonly ProcessComponent _process;
        private readonly ConcurrentDictionary<int, UserCache> _usersCached;

        public CacheManager()
        {
            _usersCached = new ConcurrentDictionary<int, UserCache>();
            _process = new ProcessComponent();
            _process.Init();
            log.Info("Cache Manager -> LOADED");
        }

        public bool ContainsUser(int Id) => _usersCached.ContainsKey(Id);

        public UserCache GenerateUser(int Id)
        {
            UserCache User = null;
            if (_usersCached.ContainsKey(Id))
            {
                if (TryGetUser(Id, out User))
                {
                    return User;
                }
            }

            var Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Id);
            if (Client != null)
            {
                if (Client.GetHabbo() != null)
                {
                    User = new UserCache(Id, Client.GetHabbo().Username, Client.GetHabbo().Motto, Client.GetHabbo().Look);
                    _usersCached.TryAdd(Id, User);
                    return User;
                }
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `username`, `motto`, `look` FROM users WHERE id = @id LIMIT 1");
                dbClient.AddParameter("id", Id);
                var dRow = dbClient.GetRow();
                if (dRow != null)
                {
                    User = new UserCache(Id, dRow["username"].ToString(), dRow["motto"].ToString(), dRow["look"].ToString());
                    _usersCached.TryAdd(Id, User);
                }
                dRow = null;
            }
            return User;
        }

        public bool TryRemoveUser(int Id, out UserCache User) => _usersCached.TryRemove(Id, out User);

        public bool TryGetUser(int Id, out UserCache User) => _usersCached.TryGetValue(Id, out User);

        public ICollection<UserCache> GetUserCache() => _usersCached.Values;
    }
}