namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    using GameClients;

    internal class SetMaxCommand : IChatCommand
    {
        public string PermissionRequired => "command_setmax";

        public string Parameters => "%value%";

        public string Description => "Set the visitor limit to the room.";

        public void Execute(GameClient session, Room room, string[] Params)
        {
            if (!room.CheckRights(session, true))
            {
                return;
            }

            if (Params.Length == 1)
            {
                session.SendWhisper("Please enter a value for the room visitor limit.");
                return;
            }

            int maxAmount;
            if (int.TryParse(Params[1], out maxAmount))
            {
                if (maxAmount == 0)
                {
                    maxAmount = 10;
                    session.SendWhisper("visitor amount too low, visitor amount has been set to 10.");
                }
                else if (maxAmount > 200 && !session.GetHabbo().GetPermissions().HasRight("override_command_setmax_limit"))
                {
                    maxAmount = 200;
                    session.SendWhisper("visitor amount too high for your rank, visitor amount has been set to 200.");
                }
                else
                {
                    session.SendWhisper("visitor amount set to " + maxAmount + ".");
                }
                room.UsersMax = maxAmount;
                room.RoomData.UsersMax = maxAmount;
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `rooms` SET `users_max` = " + maxAmount + " WHERE `id` = '" + room.Id +
                                      "' LIMIT 1");
                }
            }
            else
            {
                session.SendWhisper("Invalid amount, please enter a valid number.");
            }
        }
    }
}