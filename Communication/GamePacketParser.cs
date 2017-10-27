namespace Plus.Communication
{
    using System;
    using System.IO;
    using ConnectionManager;
    using HabboHotel.GameClients;
    using Packets.Incoming;
    using Utilities;

    public sealed class GamePacketParser : IDataParser
    {
        public delegate void HandlePacket(ClientPacket message);

        private readonly GameClient _currentClient;
        private bool _deciphered;
        private byte[] _halfData;

        private bool _halfDataRecieved;

        internal GamePacketParser(GameClient me) => _currentClient = me;

        public void HandlePacketData(byte[] data)
        {
            try
            {
                if (_currentClient.Rc4Client != null && !_deciphered)
                {
                    _currentClient.Rc4Client.Decrypt(ref data);
                    _deciphered = true;
                }
                if (_halfDataRecieved)
                {
                    var fullDataRcv = new byte[_halfData.Length + data.Length];
                    Buffer.BlockCopy(_halfData, 0, fullDataRcv, 0, _halfData.Length);
                    Buffer.BlockCopy(data, 0, fullDataRcv, _halfData.Length, data.Length);
                    _halfDataRecieved = false; // mark done this round
                    HandlePacketData(fullDataRcv); // repeat now we have the combined array
                    return;
                }

                using (var reader = new BinaryReader(new MemoryStream(data)))
                {
                    if (data.Length < 4)
                    {
                        return;
                    }

                    var msgLen = HabboEncoding.DecodeInt32(reader.ReadBytes(4));
                    if (reader.BaseStream.Length - 4 < msgLen)
                    {
                        _halfData = data;
                        _halfDataRecieved = true;
                        return;
                    }

                    if (msgLen < 0 || msgLen > 5120) //TODO: Const somewhere.
                    {
                        return;
                    }

                    var packet = reader.ReadBytes(msgLen);
                    using (var r = new BinaryReader(new MemoryStream(packet)))
                    {
                        var header = HabboEncoding.DecodeInt16(r.ReadBytes(2));
                        var content = new byte[packet.Length - 2];
                        Buffer.BlockCopy(packet, 2, content, 0, packet.Length - 2);
                        var message = new ClientPacket(header, content);

                        OnNewPacket?.Invoke(message);

                        _deciphered = false;
                    }

                    if (reader.BaseStream.Length - 4 <= msgLen)
                    {
                        return;
                    }

                    var extra = new byte[reader.BaseStream.Length - reader.BaseStream.Position];

                    Buffer.BlockCopy(data,
                        (int) reader.BaseStream.Position,
                        extra,
                        0,
                        (int) (reader.BaseStream.Length - reader.BaseStream.Position));

                    _deciphered = true;
                    HandlePacketData(extra);
                }
            }
            catch
            {
                // ignored
            }
        }

        public void Dispose()
        {
            OnNewPacket = null;
            GC.SuppressFinalize(this);
        }

        public object Clone() => new GamePacketParser(_currentClient);

        public event HandlePacket OnNewPacket;

        internal void SetConnection(ConnectionInformation con)
        {
            OnNewPacket = null;
        }
    }
}