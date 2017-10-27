namespace Plus.HabboHotel.Users.Calendar
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    public sealed class CalendarComponent
    {
        private readonly List<int> _lateBoxes;

        private readonly List<int> _openedBoxes;

        public CalendarComponent()
        {
            _lateBoxes = new List<int>();
            _openedBoxes = new List<int>();
        }

        public bool Init(Habbo Player)
        {
            if (_lateBoxes.Count > 0)
            {
                _lateBoxes.Clear();
            }
            if (_openedBoxes.Count > 0)
            {
                _openedBoxes.Clear();
            }
            DataTable GetData = null;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `user_xmas15_calendar` WHERE `user_id` = @id;");
                dbClient.AddParameter("id", Player.Id);
                GetData = dbClient.GetTable();
                if (GetData != null)
                {
                    foreach (DataRow Row in GetData.Rows)
                    {
                        if (Convert.ToInt32(Row["status"]) == 0)
                        {
                            _lateBoxes.Add(Convert.ToInt32(Row["day"]));
                        }
                        else
                        {
                            _openedBoxes.Add(Convert.ToInt32(Row["day"]));
                        }
                    }
                }
            }

            return true;
        }

        public List<int> GetOpenedBoxes() => _openedBoxes;

        public List<int> GetLateBoxes() => _lateBoxes;

        public void Dispose()
        {
            _lateBoxes.Clear();
            _openedBoxes.Clear();
        }
    }
}