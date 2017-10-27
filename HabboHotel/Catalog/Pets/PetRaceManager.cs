namespace Plus.HabboHotel.Catalog.Pets
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    public class PetRaceManager
    {
        private readonly List<PetRace> _races = new List<PetRace>();

        public void Init()
        {
            if (_races.Count > 0)
            {
                _races.Clear();
            }
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `catalog_pet_races`");
                var Table = dbClient.GetTable();
                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        var Race = new PetRace(Convert.ToInt32(Row["raceid"]),
                            Convert.ToInt32(Row["color1"]),
                            Convert.ToInt32(Row["color2"]),
                            Convert.ToString(Row["has1color"]) == "1",
                            Convert.ToString(Row["has2color"]) == "1");
                        if (!_races.Contains(Race))
                        {
                            _races.Add(Race);
                        }
                    }
                }
            }
        }

        public List<PetRace> GetRacesForRaceId(int RaceId)
        {
            return _races.Where(Race => Race.RaceId == RaceId).ToList();
        }
    }
}