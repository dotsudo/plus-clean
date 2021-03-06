﻿namespace Plus.Utilities
{
    using System;

    internal static class UnixTimestamp
    {
        public static double GetNow()
        {
            var ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0);
            return ts.TotalSeconds;
        }

        public static DateTime FromUnixTimestamp(double Timestamp)
        {
            var DT = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DT = DT.AddSeconds(Timestamp);
            return DT;
        }
    }
}