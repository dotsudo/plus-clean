namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    using GameClients;

    internal class DisableGiftsCommand : IChatCommand
    {
        public string PermissionRequired => "command_disable_gifts";

        public string Parameters => "";

        public string Description =>
            "Allows you to disable the ability to receive gifts or to enable the ability to receive gifts.";

        public void Execute(GameClient session, Room room, string[] Params)
        {
            session.GetHabbo().AllowGifts = !session.GetHabbo().AllowGifts;
            session.SendWhisper("You're " + (session.GetHabbo().AllowGifts ? "now" : "no longer") + " accepting gifts.");
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `allow_gifts` = @AllowGifts WHERE `id` = '" + session.GetHabbo().Id + "'");
                dbClient.AddParameter("AllowGifts", PlusEnvironment.BoolToEnum(session.GetHabbo().AllowGifts));
                dbClient.RunQuery();
            }
        }
    }
}