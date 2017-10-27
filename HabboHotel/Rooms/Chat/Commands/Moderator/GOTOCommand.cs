namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    using GameClients;

    internal class GotoCommand : IChatCommand
    {
        public string PermissionRequired => "command_goto";
        public string Parameters => "%room_id%";
        public string Description => "";

        public void Execute(GameClient session, Room room, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("You must specify a room id!");
                return;
            }

            if (!int.TryParse(Params[1], out var roomId))
            {
                session.SendWhisper("You must enter a valid room ID");
            }
            else
            {
                var goTo = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(roomId);

                if (goTo == null)
                {
                    session.SendWhisper("This room does not exist!");
                }
                else
                {
                    session.GetHabbo().PrepareRoom(goTo.Id, "");
                }
            }
        }
    }
}