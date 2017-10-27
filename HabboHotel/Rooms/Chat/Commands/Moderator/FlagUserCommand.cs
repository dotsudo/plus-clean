namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    using Communication.Packets.Outgoing.Handshake;
    using GameClients;

    internal class FlagUserCommand : IChatCommand
    {
        public string PermissionRequired => "command_flaguser";

        public string Parameters => "%username%";

        public string Description => "Forces the specified user to change their name.";

        public void Execute(GameClient session, Room room, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Please enter the username you wish to flag.");
                return;
            }

            var targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            if (targetClient.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                session.SendWhisper("You are not allowed to flag that user.");
                return;
            }

            targetClient.GetHabbo().LastNameChange = 0;
            targetClient.GetHabbo().ChangingName = true;
            targetClient.SendNotification(
                "Please be aware that if your username is deemed as inappropriate, you will be banned without question.\r\rAlso note that Staff will NOT allow you to change your username again should you have an issue with what you have chosen.\r\rClose this window and click yourself to begin choosing a new username!");
            targetClient.SendPacket(new UserObjectComposer(targetClient.GetHabbo()));
        }
    }
}