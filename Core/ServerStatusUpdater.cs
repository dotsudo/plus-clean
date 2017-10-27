namespace Plus.Core
{
    using System;
    using System.Threading;
    using log4net;

    public class ServerStatusUpdater : IDisposable
    {
        private const int UPDATE_IN_SECS = 30;
        private static readonly ILog log = LogManager.GetLogger("Plus.Core.ServerUpdater");

        private Timer _timer;

        public void Dispose()
        {
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `server_status` SET `users_online` = '0', `loaded_rooms` = '0'");
            }
            _timer.Dispose();
            GC.SuppressFinalize(this);
        }

        public void Init()
        {
            _timer = new Timer(OnTick, null, TimeSpan.FromSeconds(UPDATE_IN_SECS), TimeSpan.FromSeconds(UPDATE_IN_SECS));
            Console.Title = "Plus Emulator - 0 users online - 0 rooms loaded - 0 day(s) 0 hour(s) uptime";
            log.Info("Server Status Updater has been started.");
        }

        public void OnTick(object Obj)
        {
            UpdateOnlineUsers();
        }

        private void UpdateOnlineUsers()
        {
            var Uptime = DateTime.Now - PlusEnvironment.ServerStarted;
            var UsersOnline = PlusEnvironment.GetGame().GetClientManager().Count;
            var RoomCount = PlusEnvironment.GetGame().GetRoomManager().Count;
            Console.Title = "Plus Emulator - " +
                            UsersOnline +
                            " users online - " +
                            RoomCount +
                            " rooms loaded - " +
                            Uptime.Days +
                            " day(s) " +
                            Uptime.Hours +
                            " hour(s) uptime";
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `server_status` SET `users_online` = @users, `loaded_rooms` = @loadedRooms LIMIT 1;");
                dbClient.AddParameter("users", UsersOnline);
                dbClient.AddParameter("loadedRooms", RoomCount);
                dbClient.RunQuery();
            }
        }
    }
}