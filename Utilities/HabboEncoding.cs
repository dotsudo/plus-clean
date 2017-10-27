﻿namespace Plus.Utilities
{
    using System;

    public static class HabboEncoding
    {
        /* Encoding Functions */
        public static string EncodeInt32(int v)
        {
            var t = "";
            return t + (char) (v >> 0x18) + (char) (v >> 0x10) + (char) (v >> 8) + (char) v;
        }

        public static string EncodeInt16(int v)
        {
            var t = "";
            return t + (char) (v >> 8) + (char) v;
        }

        /* Decoding Functions */
        public static int DecodeInt32(string v)
        {
            if ((v[0] | v[1] | v[2] | v[3]) < 0)
            {
                return -1;
            }

            return (v[0] << 0x18) + (v[1] << 0x10) + (v[2] << 8) + v[3];
        }

        public static int DecodeInt32(byte[] v)
        {
            if ((v[0] | v[1] | v[2] | v[3]) < 0)
            {
                return -1;
            }

            return (v[0] << 0x18) + (v[1] << 0x10) + (v[2] << 8) + v[3];
        }

        public static int DecodeInt16(byte[] v)
        {
            if ((v[0] | v[1]) < 0)
            {
                return -1;
            }

            return (v[0] << 8) + v[1];
        }

        public static short DecodeInt16(string v)
        {
            if ((v[0] | v[1]) < 0)
            {
                return -1;
            }

            return (short) ((v[0] << 8) + v[1]);
        }

        public static bool DecodeBool(string v)
        {
            try
            {
                var i = Convert.ToInt32(Convert.ToChar(v.Substring(0, 1)));
                return i == 1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}