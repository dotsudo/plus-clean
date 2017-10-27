namespace Plus.HabboHotel.Rooms.Chat.Filter
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text.RegularExpressions;

    public sealed class WordFilterManager
    {
        private readonly List<WordFilter> _filteredWords;

        public WordFilterManager() => _filteredWords = new List<WordFilter>();

        public void Init()
        {
            if (_filteredWords.Count > 0)
            {
                _filteredWords.Clear();
            }
            DataTable Data = null;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `wordfilter`");
                Data = dbClient.GetTable();
                if (Data != null)
                {
                    foreach (DataRow Row in Data.Rows)
                    {
                        _filteredWords.Add(new WordFilter(Convert.ToString(Row["word"]),
                            Convert.ToString(Row["replacement"]),
                            PlusEnvironment.EnumToBool(Row["strict"].ToString()),
                            PlusEnvironment.EnumToBool(Row["bannable"].ToString())));
                    }
                }
            }
        }

        public string CheckMessage(string Message)
        {
            foreach (var Filter in _filteredWords.ToList())
            {
                if (Message.ToLower().Contains(Filter.Word) && Filter.IsStrict || Message == Filter.Word)
                {
                    Message = Regex.Replace(Message, Filter.Word, Filter.Replacement, RegexOptions.IgnoreCase);
                }
                else if (Message.ToLower().Contains(Filter.Word) && !Filter.IsStrict || Message == Filter.Word)
                {
                    var Words = Message.Split(' ');
                    Message = "";
                    foreach (var Word in Words.ToList())
                    {
                        if (Word.ToLower() == Filter.Word)
                        {
                            Message += Filter.Replacement + " ";
                        }
                        else
                        {
                            Message += Word + " ";
                        }
                    }
                }
            }

            return Message.TrimEnd(' ');
        }

        public bool CheckBannedWords(string Message)
        {
            Message = Message.Replace(" ", "").Replace(".", "").Replace("_", "").ToLower();
            foreach (var Filter in _filteredWords.ToList())
            {
                if (!Filter.IsBannable)
                {
                    continue;
                }

                if (Message.Contains(Filter.Word))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsFiltered(string Message)
        {
            foreach (var Filter in _filteredWords.ToList())
            {
                if (Message.Contains(Filter.Word))
                {
                    return true;
                }
            }

            return false;
        }
    }
}