namespace Plus.Communication.ConnectionManager
{
    using System;
    using System.Collections.Concurrent;
    using System.Net;
    using System.Net.Sockets;
    using log4net;
    using Socket_Exceptions;

    public class SocketManager
    {
        public delegate void ConnectionEvent(ConnectionInformation connection);

        private static readonly ILog Log = LogManager.GetLogger("Plus.Communication.ConnectionManager");

        private bool _acceptConnections;

        private int _acceptedConnections;

        private Socket _connectionListener;

        private bool _disableNagleAlgorithm;

        private ConcurrentDictionary<string, int> _ipConnectionsCount;

        private int _maxIpConnectionCount;

        private int _portInformation;

        private IDataParser parser;

        public event ConnectionEvent connectionEvent;

        internal void Init(int portId, int connectionsPerIp, IDataParser parser, bool disableNaglesAlgorithm)
        {
            _ipConnectionsCount = new ConcurrentDictionary<string, int>();
            this.parser = parser;
            _disableNagleAlgorithm = disableNaglesAlgorithm;
            _portInformation = portId;
            _maxIpConnectionCount = connectionsPerIp;
            PrepareConnectionDetails();
            _acceptedConnections = 0;
            Log.Info("Successfully setup GameSocketManager on port (" + portId + ")!");
            Log.Info("Maximum connections per IP has been set to [" + connectionsPerIp + "]!");
        }

        private void PrepareConnectionDetails()
        {
            _connectionListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = _disableNagleAlgorithm
            };

            try
            {
                _connectionListener.Bind(new IPEndPoint(IPAddress.Any, _portInformation));
            }
            catch (SocketException ex)
            {
                throw new SocketInitializationException(ex.Message);
            }
        }

        internal void InitializeConnectionRequests()
        {
            //Out.writeLine("Starting to listen to connection requests", Out.logFlags.ImportantLogLevel);
            _connectionListener.Listen(100);
            _acceptConnections = true;
            try
            {
                _connectionListener.BeginAccept(NewConnectionRequest, _connectionListener);
            }
            catch
            {
                Destroy();
            }
        }

        internal void Destroy()
        {
            _acceptConnections = false;

            try
            {
                _connectionListener.Close();
            }
            catch
            {
                // ignored
            }

            _connectionListener = null;
        }

        private void NewConnectionRequest(IAsyncResult iAr)
        {
            if (_connectionListener != null)
            {
                if (_acceptConnections)
                {
                    try
                    {
                        var replyFromComputer = ((Socket) iAr.AsyncState).EndAccept(iAr);
                        replyFromComputer.NoDelay = _disableNagleAlgorithm;
                        var Ip = replyFromComputer.RemoteEndPoint.ToString().Split(':')[0];
                        var connectionCount = GetAmountOfConnectionFromIp(Ip);
                        if (connectionCount < _maxIpConnectionCount)
                        {
                            _acceptedConnections++;
                            var c = new ConnectionInformation(_acceptedConnections, replyFromComputer,
                                parser.Clone() as IDataParser, Ip);
                            ReportUserLogin(Ip);
                            c.ConnectionChanged += c_connectionChanged;
                            connectionEvent?.Invoke(c);
                        }
                        else
                        {
                            Log.Info("Connection denied from [" +
                                     replyFromComputer.RemoteEndPoint.ToString().Split(':')[0] +
                                     "]. Too many connections (" +
                                     connectionCount +
                                     ").");
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                    finally
                    {
                        _connectionListener.BeginAccept(NewConnectionRequest, _connectionListener);
                    }
                }
            }
        }

        private void c_connectionChanged(ConnectionInformation information, ConnectionState state)
        {
            if (state == ConnectionState.Closed)
            {
                reportDisconnect(information);
            }
        }

        public void reportDisconnect(ConnectionInformation gameConnection)
        {
            gameConnection.ConnectionChanged -= c_connectionChanged;
            ReportUserLogout(gameConnection.GetIp());
        }

        private void ReportUserLogin(string ip)
        {
            AlterIpConnectionCount(ip, GetAmountOfConnectionFromIp(ip) + 1);
        }

        private void ReportUserLogout(string ip)
        {
            AlterIpConnectionCount(ip, GetAmountOfConnectionFromIp(ip) - 1);
        }

        private void AlterIpConnectionCount(string ip, int amount)
        {
            if (_ipConnectionsCount.ContainsKey(ip))
            {
                int am;
                _ipConnectionsCount.TryRemove(ip, out am);
            }
            _ipConnectionsCount.TryAdd(ip, amount);
        }

        private int GetAmountOfConnectionFromIp(string ip)
        {
            if (_ipConnectionsCount.ContainsKey(ip))
            {
                return _ipConnectionsCount[ip];
            }

            return 0;
        }
    }
}