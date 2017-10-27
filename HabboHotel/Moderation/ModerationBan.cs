namespace Plus.HabboHotel.Moderation
{
    public class ModerationBan
    {
        public double Expire;
        public string Reason;
        public ModerationBanType Type;
        public string Value;

        public ModerationBan(ModerationBanType type, string value, string reason, double expire)
        {
            Type = type;
            Value = value;
            Reason = reason;
            Expire = expire;
        }

        public bool Expired
        {
            get
            {
                if (PlusEnvironment.GetUnixTimestamp() >= Expire)
                {
                    return true;
                }

                return false;
            }
        }
    }
}