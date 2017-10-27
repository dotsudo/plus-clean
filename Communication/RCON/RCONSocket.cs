namespace Plus.Communication.RCON
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using Commands;

    public class RconSocket
    {
        private readonly List<string> _allowedConnections;
        private readonly CommandManager _commands;
        private readonly int _musPort;
        private readonly Socket _musSocket;

        private string _musIp;

        public RconSocket(string musIp, int musPort, string[] allowedConnections)
        {
            _musIp = musIp;
            _musPort = musPort;
            _allowedConnections = new List<string>();
            foreach (var ipAddress in allowedConnections)
            {
                _allowedConnections.Add(ipAddress);
            }

            try
            {
                _musSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _musSocket.Bind(new IPEndPoint(IPAddress.Any, _musPort));
                _musSocket.Listen(0);
                _musSocket.BeginAccept(OnCallBack, _musSocket);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Could not set up RCON socket:\n" + e);
            }

            _commands = new CommandManager();
        }

        private void OnCallBack(IAsyncResult iAr)
        {
            try
            {
                var socket = ((Socket) iAr.AsyncState).EndAccept(iAr);
                var ip = socket.RemoteEndPoint.ToString().Split(':')[0];
                if (_allowedConnections.Contains(ip))
                {
                    new RconConnection(socket);
                }
                else
                {
                    socket.Close();
                }
            }
            catch (Exception)
            {
            }
            _musSocket.BeginAccept(OnCallBack, _musSocket);
        }

        public CommandManager GetCommands() => _commands;
    }
}