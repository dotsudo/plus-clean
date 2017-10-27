namespace Plus.Communication.Packets.Incoming
{
    using System;
    using Utilities;

    public class ClientPacket
    {
        private byte[] _body;
        private int _pointer;

        internal ClientPacket(int messageId, byte[] body)
        {
            Init(messageId, body);
        }

        internal int Id { get; private set; }

        private int RemainingLength => _body.Length - _pointer;

        private int Header => Id;

        private void Init(int messageId, byte[] body)
        {
            if (body == null)
            {
                body = new byte[0];
            }
            Id = messageId;
            _body = body;
            _pointer = 0;
        }

        public override string ToString() => "[" + Header + "] BODY: " +
                                             PlusEnvironment.GetDefaultEncoding().GetString(_body)
                                                 .Replace(Convert.ToChar(0).ToString(), "[0]");

        private byte[] ReadBytes(int bytes)
        {
            if (bytes > RemainingLength)
            {
                bytes = RemainingLength;
            }
            var data = new byte[bytes];
            for (var i = 0; i < bytes; i++)
            {
                data[i] = _body[_pointer++];
            }

            return data;
        }

        private byte[] PlainReadBytes(int bytes)
        {
            if (bytes > RemainingLength)
            {
                bytes = RemainingLength;
            }
            var data = new byte[bytes];
            for (int x = 0, y = _pointer; x < bytes; x++, y++)
            {
                data[x] = _body[y];
            }

            return data;
        }

        public byte[] ReadFixedValue()
        {
            var len = HabboEncoding.DecodeInt16(ReadBytes(2));
            return ReadBytes(len);
        }

        public string PopString() => PlusEnvironment.GetDefaultEncoding().GetString(ReadFixedValue());

        public bool PopBoolean() => RemainingLength > 0 && _body[_pointer++] == Convert.ToChar(1);

        public int PopInt()
        {
            if (RemainingLength < 1)
            {
                return 0;
            }

            var data = PlainReadBytes(4);
            var i = HabboEncoding.DecodeInt32(data);
            _pointer += 4;
            return i;
        }
    }
}