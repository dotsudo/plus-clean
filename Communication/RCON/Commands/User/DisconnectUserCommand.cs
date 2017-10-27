namespace Plus.Communication.RCON.Commands.User
{
    internal class DisconnectUserCommand : IRconCommand
    {
        public string Description => "This command is used to disconnect a user.";

        public string Parameters => "%userId%";

        public bool TryExecute(string[] parameters)
        {
            var userId = 0;
            if (!int.TryParse(parameters[0], out userId))
            {
                return false;
            }

            var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(userId);
            if (client == null || client.GetHabbo() == null)
            {
                return false;
            }

            client.Disconnect();
            return true;
        }
    }
}