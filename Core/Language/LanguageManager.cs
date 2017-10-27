namespace Plus.Core.Language
{
    using System.Collections.Generic;
    using System.Data;
    using log4net;

    public class LanguageManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.Core.Language.LanguageManager");
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>();

        public LanguageManager() => _values = new Dictionary<string, string>();

        public void Init()
        {
            if (_values.Count > 0)
            {
                _values.Clear();
            }
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `server_locale`");
                var Table = dbClient.GetTable();
                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        _values.Add(Row["key"].ToString(), Row["value"].ToString());
                    }
                }
            }

            log.Info("Loaded " + _values.Count + " language locales.");
        }

        public string TryGetValue(string value) =>
            _values.ContainsKey(value) ? _values[value] : "No language locale found for [" + value + "]";
    }
}