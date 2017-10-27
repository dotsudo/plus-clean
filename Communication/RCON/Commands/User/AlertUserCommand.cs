namespace Plus.Communication.RCON.Commands.User
{
    using System;
    using Packets.Outgoing.Moderation;

    internal class AlertUserCommand : IRconCommand
    {
        public string Description => "This command is used to alert a user.";

        public string Parameters => "%userId% %message%";

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

            // Validate the message
            if (string.IsNullOrEmpty(Convert.ToString(parameters[1])))
            {
                return false;
            }

            var message = Convert.ToString(parameters[1]);
            client.SendPacket(new BroadcastMessageAlertComposer(message));
            return true;
        }
    }
}