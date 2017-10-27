namespace Plus.HabboHotel.Users.Authenticator
{
    using System;

    [Serializable]
    public class IncorrectLoginException : Exception
    {
        public IncorrectLoginException(string Reason) : base(Reason)
        {
        }
    }
}