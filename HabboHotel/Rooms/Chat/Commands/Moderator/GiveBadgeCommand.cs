namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    using GameClients;

    internal class GiveBadgeCommand : IChatCommand
    {
        public string PermissionRequired => "command_give_badge";

        public string Parameters => "%username% %badge%";

        public string Description => "Give a badge to another user.";

        public void Execute(GameClient session, Room room, string[] Params)
        {
            if (Params.Length != 3)
            {
                session.SendWhisper("Please enter a username and the code of the badge you'd like to give!");
                return;
            }

            var targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (targetClient != null)
            {
                if (!targetClient.GetHabbo().GetBadgeComponent().HasBadge(Params[2]))
                {
                    targetClient.GetHabbo().GetBadgeComponent().GiveBadge(Params[2], true, targetClient);
                    if (targetClient.GetHabbo().Id != session.GetHabbo().Id)
                    {
                        targetClient.SendNotification("You have just been given a badge!");
                    }
                    else
                    {
                        session.SendWhisper("You have successfully given yourself the badge " + Params[2] + "!");
                    }
                }
                else
                {
                    session.SendWhisper("Oops, that user already has this badge (" + Params[2] + ") !");
                }
                return;
            }

            session.SendWhisper("Oops, we couldn't find that target user!");
        }
    }
}