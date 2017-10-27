namespace Plus.Utilities
{
    using System;

    internal static class Converter
    {
        internal static string BytesToHexString(byte[] bytes)
        {
            var hexstring = BitConverter.ToString(bytes);
            return hexstring.Replace("-", "");
        }

        internal static byte[] HexStringToBytes(string hexstring)
        {
            var numberChars = hexstring.Length;
            var bytes = new byte[numberChars / 2];

            for (var i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexstring.Substring(i, 2), 16);
            }

            return bytes;
        }
    }
}