namespace Plus.HabboHotel.Rooms.Chat
{
    using Commands;
    using Emotions;
    using Filter;
    using log4net;
    using Logs;
    using Pets.Commands;
    using Pets.Locale;
    using Styles;

    public sealed class ChatManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Rooms.Chat.ChatManager");

        private readonly ChatStyleManager _chatStyles;

        private readonly CommandManager _commands;

        private readonly ChatEmotionsManager _emotions;

        private readonly WordFilterManager _filter;

        private readonly ChatlogManager _logs;

        private readonly PetCommandManager _petCommands;

        private readonly PetLocale _petLocale;

        public ChatManager()
        {
            _emotions = new ChatEmotionsManager();
            _logs = new ChatlogManager();
            _filter = new WordFilterManager();
            _filter.Init();
            _commands = new CommandManager(":");
            _petCommands = new PetCommandManager();
            _petLocale = new PetLocale();
            _chatStyles = new ChatStyleManager();
            _chatStyles.Init();
            log.Info("Chat Manager -> LOADED");
        }

        public ChatEmotionsManager GetEmotions() => _emotions;

        public ChatlogManager GetLogs() => _logs;

        public WordFilterManager GetFilter() => _filter;

        public CommandManager GetCommands() => _commands;

        public PetCommandManager GetPetCommands() => _petCommands;

        public PetLocale GetPetLocale() => _petLocale;

        public ChatStyleManager GetChatStyles() => _chatStyles;
    }
}