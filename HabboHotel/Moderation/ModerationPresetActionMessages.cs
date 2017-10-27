namespace Plus.HabboHotel.Moderation
{
    internal class ModerationPresetActionMessages
    {
        internal ModerationPresetActionMessages(int id,
                                                int parentId,
                                                string caption,
                                                string messageText,
                                                int muteTime,
                                                int banTime,
                                                int ipBanTime,
                                                int tradeLockTime,
                                                string notice)
        {
            Id = id;
            Caption = caption;
            MessageText = messageText;
            MuteTime = muteTime;
            BanTime = banTime;
            IpBanTime = ipBanTime;
            TradeLockTime = tradeLockTime;
            Notice = notice;
        }

        private int Id { get; }
        private string Caption { get; }
        private string MessageText { get; }
        private int MuteTime { get; }
        private int BanTime { get; }
        private int IpBanTime { get; }
        private int TradeLockTime { get; }
        private string Notice { get; }
    }
}