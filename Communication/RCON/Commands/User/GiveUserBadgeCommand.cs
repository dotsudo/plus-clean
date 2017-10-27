namespace Plus.Communication.RCON.Commands.User
{
    using System;
    using Packets.Outgoing.Moderation;

    internal class GiveUserBadgeCommand : IRconCommand
    {
        public string Description => "This command is used to give a user a badge.";

        public string Parameters => "%userId% %badgeId%";

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

            // Validate the badge
            if (string.IsNullOrEmpty(Convert.ToString(parameters[1])))
            {
                return false;
            }

            var badge = Convert.ToString(parameters[1]);
            if (!client.GetHabbo().GetBadgeComponent().HasBadge(badge))
            {
                client.GetHabbo().GetBadgeComponent().GiveBadge(badge, true, client);
                client.SendPacket(new BroadcastMessageAlertComposer("You have been given a new badge!"));
            }
            return true;
        }
    }
}