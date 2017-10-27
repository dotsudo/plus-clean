namespace Plus.HabboHotel.Badges
{
    public class BadgeDefinition
    {
        public BadgeDefinition(string Code, string RequiredRight)
        {
            this.Code = Code;
            this.RequiredRight = RequiredRight;
        }

        public string Code { get; set; }

        public string RequiredRight { get; set; }
    }
}