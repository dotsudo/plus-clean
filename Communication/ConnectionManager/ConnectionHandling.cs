namespace Plus.Communication.ConnectionManager
{
    using System;
    using Core;

    public class ConnectionHandling
    {
        private readonly SocketManager _manager;

        internal ConnectionHandling(int port, int connectionsPerIp, bool enabeNagles)
        {
            _manager = new SocketManager();
            _manager.Init(port, connectionsPerIp, new InitialPacketParser(), !enabeNagles);
        }

        internal void Init()
        {
            _manager.connectionEvent += manager_connectionEvent;
            _manager.InitializeConnectionRequests();
        }

        private void manager_connectionEvent(ConnectionInformation connection)
        {
            connection.ConnectionChanged += ConnectionChanged;
            PlusEnvironment.GetGame().GetClientManager()
                .CreateAndStartClient(Convert.ToInt32(connection.GetConnectionId()), connection);
        }

        private void ConnectionChanged(ConnectionInformation information, ConnectionState state)
        {
            if (state == ConnectionState.Closed)
            {
                CloseConnection(information);
            }
        }

        private void CloseConnection(ConnectionInformation connection)
        {
            try
            {
                connection.Dispose();
                PlusEnvironment.GetGame().GetClientManager().DisposeConnection(Convert.ToInt32(connection.GetConnectionId()));
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        internal void Destroy()
        {
            _manager.Destroy();
        }
    }
}