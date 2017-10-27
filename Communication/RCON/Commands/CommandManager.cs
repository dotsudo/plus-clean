namespace Plus.Communication.RCON.Commands
{
    using System;
    using System.Collections.Generic;
    using Hotel;
    using User;

    public class CommandManager
    {
        private readonly Dictionary<string, IRconCommand> _commands;

        public CommandManager()
        {
            _commands = new Dictionary<string, IRconCommand>();
            RegisterUser();
            RegisterHotel();
        }

        public bool Parse(string data)
        {
            if (data.Length == 0 || string.IsNullOrEmpty(data))
            {
                return false;
            }

            var cmd = data.Split(Convert.ToChar(1))[0];
            IRconCommand command;
            if (_commands.TryGetValue(cmd.ToLower(), out command))
            {
                string param;
                string[] parameters = null;
                if (data.Split(Convert.ToChar(1))[1] != null)
                {
                    param = data.Split(Convert.ToChar(1))[1];
                    parameters = param.Split(':');
                }
                return command.TryExecute(parameters);
            }

            return false;
        }

        private void RegisterUser()
        {
            Register("alert_user", new AlertUserCommand());
            Register("disconnect_user", new DisconnectUserCommand());
            Register("reload_user_motto", new ReloadUserMottoCommand());
            Register("give_user_currency", new GiveUserCurrencyCommand());
            Register("take_user_currency", new TakeUserCurrencyCommand());
            Register("sync_user_currency", new SyncUserCurrencyCommand());
            Register("reload_user_currency", new ReloadUserCurrencyCommand());
            Register("reload_user_rank", new ReloadUserRankCommand());
            Register("reload_user_vip_rank", new ReloadUserVipRankCommand());
            Register("progress_user_achievement", new ProgressUserAchievementCommand());
            Register("give_user_badge", new GiveUserBadgeCommand());
            Register("take_user_badge", new TakeUserBadgeCommand());
        }

        private void RegisterHotel()
        {
            Register("reload_bans", new ReloadBansCommand());
            Register("reload_quests", new ReloadQuestsCommand());
            Register("reload_server_settings", new ReloadServerSettingsCommand());
            Register("reload_vouchers", new ReloadVouchersCommand());
            Register("reload_ranks", new ReloadRanksCommand());
            Register("reload_navigator", new ReloadNavigatorCommand());
            Register("reload_items", new ReloadItemsCommand());
            Register("reload_catalog", new ReloadCatalogCommand());
            Register("reload_filter", new ReloadFilterCommand());
        }

        public void Register(string commandText, IRconCommand command)
        {
            _commands.Add(commandText, command);
        }
    }
}