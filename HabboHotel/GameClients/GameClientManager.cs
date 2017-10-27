namespace Plus.HabboHotel.GameClients
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using Communication.ConnectionManager;
    using Communication.Packets.Outgoing;
    using Communication.Packets.Outgoing.Handshake;
    using Communication.Packets.Outgoing.Notifications;
    using Core;
    using log4net;
    using Users.Messenger;

    public class GameClientManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.GameClients.GameClientManager");

        private readonly ConcurrentDictionary<int, GameClient> _clients;
        private readonly ConcurrentDictionary<int, GameClient> _userIDRegister;
        private readonly ConcurrentDictionary<string, GameClient> _usernameRegister;

        private readonly Stopwatch clientPingStopwatch;

        private readonly Queue timedOutConnections;

        public GameClientManager()
        {
            _clients = new ConcurrentDictionary<int, GameClient>();
            _userIDRegister = new ConcurrentDictionary<int, GameClient>();
            _usernameRegister = new ConcurrentDictionary<string, GameClient>();
            timedOutConnections = new Queue();
            clientPingStopwatch = new Stopwatch();
            clientPingStopwatch.Start();
        }

        public int Count => _clients.Count;

        public ICollection<GameClient> GetClients => _clients.Values;

        public void OnCycle()
        {
            TestClientConnections();
            HandleTimeouts();
        }

        public GameClient GetClientByUserID(int userID)
        {
            if (_userIDRegister.ContainsKey(userID))
            {
                return _userIDRegister[userID];
            }

            return null;
        }

        public GameClient GetClientByUsername(string username)
        {
            if (_usernameRegister.ContainsKey(username.ToLower()))
            {
                return _usernameRegister[username.ToLower()];
            }

            return null;
        }

        public bool TryGetClient(int ClientId, out GameClient Client) => _clients.TryGetValue(ClientId, out Client);

        public bool UpdateClientUsername(GameClient Client, string OldUsername, string NewUsername)
        {
            if (Client == null || !_usernameRegister.ContainsKey(OldUsername.ToLower()))
            {
                return false;
            }

            _usernameRegister.TryRemove(OldUsername.ToLower(), out Client);
            _usernameRegister.TryAdd(NewUsername.ToLower(), Client);
            return true;
        }

        public string GetNameById(int Id)
        {
            var client = GetClientByUserID(Id);
            if (client != null)
            {
                return client.GetHabbo().Username;
            }

            string username;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT username FROM users WHERE id = @id LIMIT 1");
                dbClient.AddParameter("id", Id);
                username = dbClient.GetString();
            }
            return username;
        }

        public IEnumerable<GameClient> GetClientsById(Dictionary<int, MessengerBuddy>.KeyCollection users)
        {
            foreach (var id in users)
            {
                var client = GetClientByUserID(id);
                if (client != null)
                {
                    yield return client;
                }
            }
        }

        public void StaffAlert(ServerPacket Message, int Exclude = 0)
        {
            foreach (var client in GetClients.ToList())
            {
                if (client == null || client.GetHabbo() == null)
                {
                    continue;
                }
                if (client.GetHabbo().Rank < 2 || client.GetHabbo().Id == Exclude)
                {
                    continue;
                }

                client.SendPacket(Message);
            }
        }

        public void ModAlert(string Message)
        {
            foreach (var client in GetClients.ToList())
            {
                if (client == null || client.GetHabbo() == null)
                {
                    continue;
                }

                if (client.GetHabbo().GetPermissions().HasRight("mod_tool") &&
                    !client.GetHabbo().GetPermissions().HasRight("staff_ignore_mod_alert"))
                {
                    try
                    {
                        client.SendWhisper(Message, 5);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public void DoAdvertisingReport(GameClient Reporter, GameClient Target)
        {
            if (Reporter == null || Target == null || Reporter.GetHabbo() == null || Target.GetHabbo() == null)
            {
                return;
            }

            var Builder = new StringBuilder();
            Builder.Append("New report submitted!\r\r");
            Builder.Append("Reporter: " + Reporter.GetHabbo().Username + "\r");
            Builder.Append("Reported User: " + Target.GetHabbo().Username + "\r\r");
            Builder.Append(Target.GetHabbo().Username + "s last 10 messages:\r\r");
            DataTable GetLogs = null;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `message` FROM `chatlogs` WHERE `user_id` = '" + Target.GetHabbo().Id +
                                  "' ORDER BY `id` DESC LIMIT 10");
                GetLogs = dbClient.GetTable();
                if (GetLogs != null)
                {
                    var Number = 11;
                    foreach (DataRow Log in GetLogs.Rows)
                    {
                        Number -= 1;
                        Builder.Append(Number + ": " + Convert.ToString(Log["message"]) + "\r");
                    }
                }
            }

            foreach (var Client in GetClients.ToList())
            {
                if (Client == null || Client.GetHabbo() == null)
                {
                    continue;
                }

                if (Client.GetHabbo().GetPermissions().HasRight("mod_tool") &&
                    !Client.GetHabbo().GetPermissions().HasRight("staff_ignore_advertisement_reports"))
                {
                    Client.SendPacket(new MotdNotificationComposer(Builder.ToString()));
                }
            }
        }

        public void SendPacket(ServerPacket Packet, string fuse = "")
        {
            foreach (var Client in _clients.Values.ToList())
            {
                if (Client == null || Client.GetHabbo() == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(fuse))
                {
                    if (!Client.GetHabbo().GetPermissions().HasRight(fuse))
                    {
                        continue;
                    }
                }

                Client.SendPacket(Packet);
            }
        }

        public void CreateAndStartClient(int clientID, ConnectionInformation connection)
        {
            var Client = new GameClient(clientID, connection);
            if (_clients.TryAdd(Client.ConnectionId, Client))
            {
                Client.StartConnection();
            }
            else
            {
                connection.Dispose();
            }
        }

        public void DisposeConnection(int clientID)
        {
            GameClient Client = null;
            if (!TryGetClient(clientID, out Client))
            {
                return;
            }

            if (Client != null)
            {
                Client.Dispose();
            }
            _clients.TryRemove(clientID, out Client);
        }

        public void LogClonesOut(int UserID)
        {
            var client = GetClientByUserID(UserID);
            if (client != null)
            {
                client.Disconnect();
            }
        }

        public void RegisterClient(GameClient client, int userID, string username)
        {
            if (_usernameRegister.ContainsKey(username.ToLower()))
            {
                _usernameRegister[username.ToLower()] = client;
            }
            else
            {
                _usernameRegister.TryAdd(username.ToLower(), client);
            }
            if (_userIDRegister.ContainsKey(userID))
            {
                _userIDRegister[userID] = client;
            }
            else
            {
                _userIDRegister.TryAdd(userID, client);
            }
        }

        public void UnregisterClient(int userid, string username)
        {
            GameClient Client = null;
            _userIDRegister.TryRemove(userid, out Client);
            _usernameRegister.TryRemove(username.ToLower(), out Client);
        }

        public void CloseAll()
        {
            foreach (var client in GetClients.ToList())
            {
                if (client == null)
                {
                    continue;
                }

                if (client.GetHabbo() != null)
                {
                    try
                    {
                        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery(client.GetHabbo().GetQueryString);
                        }
                        Console.Clear();
                        log.Info("<<- SERVER SHUTDOWN ->> IVNENTORY IS SAVING");
                    }
                    catch
                    {
                    }
                }
            }

            log.Info("Done saving users inventory!");
            log.Info("Closing server connections...");
            try
            {
                foreach (var client in GetClients.ToList())
                {
                    if (client == null || client.GetConnection() == null)
                    {
                        continue;
                    }

                    try
                    {
                        client.GetConnection().Dispose();
                    }
                    catch
                    {
                    }
                    Console.Clear();
                    log.Info("<<- SERVER SHUTDOWN ->> CLOSING CONNECTIONS");
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }

            if (_clients.Count > 0)
            {
                _clients.Clear();
            }
            log.Info("Connections closed!");
        }

        private void TestClientConnections()
        {
            if (clientPingStopwatch.ElapsedMilliseconds >= 30000)
            {
                clientPingStopwatch.Restart();
                try
                {
                    var ToPing = new List<GameClient>();
                    foreach (var client in _clients.Values.ToList())
                    {
                        if (client.PingCount < 6)
                        {
                            client.PingCount++;
                            ToPing.Add(client);
                        }
                        else
                        {
                            lock (timedOutConnections.SyncRoot)
                            {
                                timedOutConnections.Enqueue(client);
                            }
                        }
                    }

                    var start = DateTime.Now;
                    foreach (var Client in ToPing.ToList())
                    {
                        try
                        {
                            Client.SendPacket(new PongComposer());
                        }
                        catch
                        {
                            lock (timedOutConnections.SyncRoot)
                            {
                                timedOutConnections.Enqueue(Client);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private void HandleTimeouts()
        {
            if (timedOutConnections.Count > 0)
            {
                lock (timedOutConnections.SyncRoot)
                {
                    while (timedOutConnections.Count > 0)
                    {
                        GameClient client = null;
                        if (timedOutConnections.Count > 0)
                        {
                            client = (GameClient) timedOutConnections.Dequeue();
                        }
                        if (client != null)
                        {
                            client.Disconnect();
                        }
                    }
                }
            }
        }
    }
}