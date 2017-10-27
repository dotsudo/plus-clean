namespace Plus.Communication.ConnectionManager
{
    using System;
    using System.Net.Sockets;

    public sealed class ConnectionInformation : IDisposable
    {
        public delegate void ConnectionChange(ConnectionInformation information, ConnectionState state);

        private static readonly bool DisableSend = false;
        private static readonly bool DisableReceive = false;

        private readonly byte[] _buffer;

        private readonly int _connectionId;

        private readonly Socket _dataSocket;

        private readonly string _ip;

        private readonly AsyncCallback _sendCallback;

        private bool _isConnected;

        internal ConnectionInformation(int connectionId, Socket dataStream, IDataParser parser, string ip)
        {
            Parser = parser;
            _buffer = new byte[GameSocketManagerStatics.BufferSize];
            _dataSocket = dataStream;
            _dataSocket.SendBufferSize = GameSocketManagerStatics.BufferSize;
            _ip = ip;
            _sendCallback = SentData;
            _connectionId = connectionId;

            ConnectionChanged?.Invoke(this, ConnectionState.Open);
        }

        internal IDataParser Parser { get; set; }

        public void Dispose()
        {
            if (_isConnected)
            {
                Disconnect();
            }

            GC.SuppressFinalize(this);
        }

        public event ConnectionChange ConnectionChanged;

        internal void StartPacketProcessing()
        {
            if (_isConnected)
            {
                return;
            }

            _isConnected = true;

            try
            {
                _dataSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, IncomingDataPacket, _dataSocket);
            }
            catch
            {
                Disconnect();
            }
        }

        internal string GetIp() => _ip;
        internal int GetConnectionId() => _connectionId;

        private void Disconnect()
        {
            try
            {
                if (!_isConnected)
                {
                    return;
                }

                _isConnected = false;

                try
                {
                    if (_dataSocket != null && _dataSocket.Connected)
                    {
                        _dataSocket.Shutdown(SocketShutdown.Both);
                        _dataSocket.Close();
                    }
                }
                catch
                {
                    // ignored
                }

                _dataSocket?.Dispose();

                Parser.Dispose();
                try
                {
                    ConnectionChanged?.Invoke(this, ConnectionState.Closed);
                }
                catch
                {
                    // ignored
                }

                ConnectionChanged = null;
            }
            catch
            {
                // ignored
            }
        }

        private void IncomingDataPacket(IAsyncResult iAr)
        {
            int bytesReceived;
            try
            {
                //The amount of bytes received in the packet
                bytesReceived = _dataSocket.EndReceive(iAr);
            }
            catch //(Exception e)
            {
                Disconnect();
                return;
            }

            if (bytesReceived == 0)
            {
                Disconnect();
                return;
            }

            try
            {
                if (DisableReceive)
                {
                    return;
                }

                var packet = new byte[bytesReceived];
                Array.Copy(_buffer, packet, bytesReceived);
                HandlePacketData(packet);
            }
            catch //(Exception e)
            {
                Disconnect();
            }
            finally
            {
                try
                {
                    _dataSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, IncomingDataPacket, _dataSocket);
                }
                catch
                {
                    Disconnect();
                }
            }
        }

        private void HandlePacketData(byte[] packet)
        {
            Parser?.HandlePacketData(packet);
        }

        internal void SendData(byte[] packet)
        {
            try
            {
                if (!_isConnected || DisableSend)
                {
                    return;
                }

                _dataSocket.BeginSend(packet, 0, packet.Length, 0, _sendCallback, null);
            }
            catch
            {
                Disconnect();
            }
        }

        private void SentData(IAsyncResult iAr)
        {
            try
            {
                _dataSocket.EndSend(iAr);
            }
            catch
            {
                Disconnect();
            }
        }
    }
}