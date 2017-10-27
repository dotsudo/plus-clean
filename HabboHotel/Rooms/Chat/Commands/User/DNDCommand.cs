namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    using GameClients;

    internal class DndCommand : IChatCommand
    {
        public string PermissionRequired => "command_dnd";

        public string Parameters => "";

        public string Description => "Allows you to chose the option to enable or disable console messages.";

        public void Execute(GameClient session, Room room, string[] Params)
        {
            session.GetHabbo().AllowConsoleMessages = !session.GetHabbo().AllowConsoleMessages;
            session.SendWhisper("You're " + (session.GetHabbo().AllowConsoleMessages ? "now" : "no longer") +
                                " accepting console messages.");
        }
    }
}