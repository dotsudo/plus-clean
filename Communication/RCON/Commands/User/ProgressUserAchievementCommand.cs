namespace Plus.Communication.RCON.Commands.User
{
    using System;

    internal class ProgressUserAchievementCommand : IRconCommand
    {
        public string Description => "This command is used to progress a users achievement.";

        public string Parameters => "%userId% %achievement% %progess%";

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

            if (string.IsNullOrEmpty(Convert.ToString(parameters[1])))
            {
                return false;
            }

            var achievement = Convert.ToString(parameters[1]);

            if (!int.TryParse(parameters[2], out var progress))
            {
                return false;
            }

            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(client, achievement, progress);
            return true;
        }
    }
}