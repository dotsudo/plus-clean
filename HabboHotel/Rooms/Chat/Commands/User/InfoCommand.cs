namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    using System;
    using Communication.Packets.Outgoing.Rooms.Notifications;
    using GameClients;

    internal class InfoCommand : IChatCommand
    {
        public string PermissionRequired => "command_info";

        public string Parameters => "";

        public string Description => "Displays generic information that everybody loves to see.";

        public void Execute(GameClient session, Room room, string[] Params)
        {
            var uptime = DateTime.Now - PlusEnvironment.ServerStarted;
            var onlineUsers = PlusEnvironment.GetGame().GetClientManager().Count;
            var roomCount = PlusEnvironment.GetGame().GetRoomManager().Count;
            session.SendPacket(new RoomNotificationComposer("Powered by PlusEmulator",
                "<b>Credits</b>:\n" +
                "DevBest Community\n\n" +
                "<b>Current run time information</b>:\n" +
                "Online Users: " +
                onlineUsers +
                "\n" +
                "Rooms Loaded: " +
                roomCount +
                "\n" +
                "Uptime: " +
                uptime.Days +
                " day(s), " +
                uptime.Hours +
                " hours and " +
                uptime.Minutes +
                " minutes.\n\n" +
                "<b>SWF Revision</b>:\n" +
                PlusEnvironment.SWFRevision,
                "plus",
                ""));
        }
    }
}