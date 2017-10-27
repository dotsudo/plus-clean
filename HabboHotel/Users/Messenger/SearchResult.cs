namespace Plus.HabboHotel.Users.Messenger
{
    public struct SearchResult
    {
        public string Figure;
        public string LastOnline;
        public string Motto;
        public int UserId;
        public string Username;

        public SearchResult(int userId, string username, string motto, string figure, string lastOnline)
        {
            UserId = userId;
            Username = username;
            Motto = motto;
            Figure = figure;
            LastOnline = lastOnline;
        }
    }
}