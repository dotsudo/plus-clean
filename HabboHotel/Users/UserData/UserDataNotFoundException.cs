namespace Plus.HabboHotel.Users.UserData
{
    using System;

    public class UserDataNotFoundException : Exception
    {
        public UserDataNotFoundException(string reason) : base(reason)
        {
        }
    }
}