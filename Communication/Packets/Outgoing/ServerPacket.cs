﻿namespace Plus.Communication.Packets.Outgoing
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Interfaces;

    public class ServerPacket : IServerPacket
    {
        private readonly List<byte> _body = new List<byte>();
        private readonly Encoding _encoding = Encoding.Default;

        public ServerPacket(int id)
        {
            Id = id;
            WriteShort(id);
        }

        public int Id { get; }

        public byte[] GetBytes()
        {
            var final = new List<byte>();
            final.AddRange(BitConverter.GetBytes(_body.Count)); // packet len
            final.Reverse();
            final.AddRange(_body); // Add Packet
            return final.ToArray();
        }

        public void WriteByte(int b)
        {
            _body.Add((byte) b);
        }

        public void WriteBytes(byte[] b, bool isInt) // d
        {
            if (isInt)
            {
                for (var i = b.Length - 1; i > -1; i--)
                {
                    _body.Add(b[i]);
                }
            }
            else
            {
                _body.AddRange(b);
            }
        }

        public void WriteDouble(double d) // d
        {
            var raw = Math.Round(d, 1).ToString();
            if (raw.Length == 1)
            {
                raw += ".0";
            }
            WriteString(raw.Replace(',', '.'));
        }

        public void WriteString(string s) // d
        {
            WriteShort(s.Length);
            WriteBytes(_encoding.GetBytes(s), false);
        }

        public void WriteShort(int s) // d
        {
            var i = (short) s;
            WriteBytes(BitConverter.GetBytes(i), true);
        }

        public void WriteInteger(int i) // d
        {
            WriteBytes(BitConverter.GetBytes(i), true);
        }

        public void WriteBoolean(bool b) // d
        {
            WriteBytes(new[] {(byte) (b ? 1 : 0)}, false);
        }
    }
}