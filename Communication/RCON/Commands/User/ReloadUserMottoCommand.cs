namespace Plus.Communication.RCON.Commands.User
{
    using Packets.Outgoing.Rooms.Engine;

    internal class ReloadUserMottoCommand : IRconCommand
    {
        public string Description => "This command is used to reload the users motto from the database.";

        public string Parameters => "%userId%";

        public bool TryExecute(string[] parameters)
        {
            if (!int.TryParse(parameters[0], out var userId))
            {
                return false;
            }

            var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(userId);

            if (client?.GetHabbo() == null)
            {
                return false;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `motto` FROM `users` WHERE `id` = @userID LIMIT 1");
                dbClient.AddParameter("userID", userId);
                client.GetHabbo().Motto = dbClient.GetString();
            }

            // If we're in a room, we cannot really send the packets, so flag this as completed successfully, since we already updated it.
            if (!client.GetHabbo().InRoom)
            {
                return true;
            }

            var room = client.GetHabbo().CurrentRoom;

            var user = room?.GetRoomUserManager().GetRoomUserByHabbo(client.GetHabbo().Id);

            if (user == null)
            {
                return false;
            }

            room.SendPacket(new UserChangeComposer(user, false));
            return true;
        }
    }
}