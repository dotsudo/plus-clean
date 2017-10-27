namespace Plus.Core.Settings
{
    using System.Collections.Generic;
    using System.Data;
    using log4net;

    public class SettingsManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.Core.Settings.SettingsManager");
        private readonly Dictionary<string, string> _settings = new Dictionary<string, string>();

        public SettingsManager() => _settings = new Dictionary<string, string>();

        public void Init()
        {
            if (_settings.Count > 0)
            {
                _settings.Clear();
            }
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `server_settings`");
                var Table = dbClient.GetTable();
                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        _settings.Add(Row["key"].ToString().ToLower(), Row["value"].ToString().ToLower());
                    }
                }
            }

            log.Info("Loaded " + _settings.Count + " server settings.");
        }

        public string TryGetValue(string value) => _settings.ContainsKey(value) ? _settings[value] : "0";
    }
}