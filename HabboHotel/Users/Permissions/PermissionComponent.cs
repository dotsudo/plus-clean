namespace Plus.HabboHotel.Users.Permissions
{
    using System.Collections.Generic;

    public sealed class PermissionComponent
    {
        private readonly List<string> _commands;
        private readonly List<string> _permissions;

        public PermissionComponent()
        {
            _permissions = new List<string>();
            _commands = new List<string>();
        }

        public bool Init(Habbo Player)
        {
            if (_permissions.Count > 0)
            {
                _permissions.Clear();
            }
            if (_commands.Count > 0)
            {
                _commands.Clear();
            }
            _permissions.AddRange(PlusEnvironment.GetGame().GetPermissionManager().GetPermissionsForPlayer(Player));
            _commands.AddRange(PlusEnvironment.GetGame().GetPermissionManager().GetCommandsForPlayer(Player));
            return true;
        }

        public bool HasRight(string Right) => _permissions.Contains(Right);

        public bool HasCommand(string Command) => _commands.Contains(Command);

        public void Dispose()
        {
            _permissions.Clear();
        }
    }
}