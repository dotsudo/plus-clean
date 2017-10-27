namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    using GameClients;

    internal class UnloadCommand : IChatCommand
    {
        public string PermissionRequired => "command_unload";

        public string Parameters => "%id%";

        public string Description => "Unload the current room.";

        public void Execute(GameClient session, Room room, string[] Params)
        {
            if (session.GetHabbo().GetPermissions().HasRight("room_unload_any"))
            {
                Room r = null;
                if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(room.Id, out r))
                {
                    return;
                }

                PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(r, true);
            }
            else
            {
                if (room.CheckRights(session, true))
                {
                    PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(room);
                }
            }
        }
    }
}