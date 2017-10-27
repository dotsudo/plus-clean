﻿namespace Plus.Core.FigureData.Types
{
    internal static class SetTypeUtility
    {
        internal static SetType GetSetType(string type)
        {
            switch (type)
            {
                default:
                    return SetType.Hr;
                case "HD":
                    return SetType.Hd;
                case "CH":
                    return SetType.Ch;
                case "LG":
                    return SetType.Lg;
                case "SH":
                    return SetType.Sh;
                case "HA":
                    return SetType.Ha;
                case "HE":
                    return SetType.He;
                case "EA":
                    return SetType.Ea;
                case "FA":
                    return SetType.Fa;
                case "CA":
                    return SetType.Ca;
                case "WA":
                    return SetType.Wa;
                case "CC":
                    return SetType.Cc;
                case "CP":
                    return SetType.Cp;
            }
        }
    }
}