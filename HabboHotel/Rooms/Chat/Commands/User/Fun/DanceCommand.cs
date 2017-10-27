namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    using Communication.Packets.Outgoing.Rooms.Avatar;
    using GameClients;

    internal class DanceCommand : IChatCommand
    {
        public string PermissionRequired => "command_dance";

        public string Parameters => "%DanceId%";

        public string Description => "Too lazy to dance the proper way? Do it like this!";

        public void Execute(GameClient session, Room room, string[] Params)
        {
            var thisUser = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (thisUser == null)
            {
                return;
            }

            if (Params.Length == 1)
            {
                session.SendWhisper("Please enter an ID of a dance.");
                return;
            }

            int danceId;
            if (int.TryParse(Params[1], out danceId))
            {
                if (danceId > 4 || danceId < 0)
                {
                    session.SendWhisper("The dance ID must be between 0 and 4!");
                    return;
                }

                session.GetHabbo().CurrentRoom.SendPacket(new DanceComposer(thisUser, danceId));
            }
            else
            {
                session.SendWhisper("Please enter a valid dance ID.");
            }
        }
    }
}