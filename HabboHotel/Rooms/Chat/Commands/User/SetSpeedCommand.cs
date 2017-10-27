namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    using GameClients;

    internal class SetSpeedCommand : IChatCommand
    {
        public string PermissionRequired => "command_setspeed";

        public string Parameters => "%value%";

        public string Description => "Set the speed of the rollers in the current room.";

        public void Execute(GameClient session, Room room, string[] Params)
        {
            if (!room.CheckRights(session, true))
            {
                return;
            }

            if (Params.Length == 1)
            {
                session.SendWhisper("Please enter a value for the roller speed.");
                return;
            }

            int speed;
            if (int.TryParse(Params[1], out speed))
            {
                session.GetHabbo().CurrentRoom.GetRoomItemHandler().SetSpeed(speed);
            }
            else
            {
                session.SendWhisper("Invalid amount, please enter a valid number.");
            }
        }
    }
}