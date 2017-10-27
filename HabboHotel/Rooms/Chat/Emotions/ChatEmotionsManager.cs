namespace Plus.HabboHotel.Rooms.Chat.Emotions
{
    using System.Collections.Generic;

    public sealed class ChatEmotionsManager
    {
        private readonly Dictionary<string, ChatEmotions> Emotions = new Dictionary<string, ChatEmotions>
        {
            // Smile
            {":)", ChatEmotions.Smile},
            {";)", ChatEmotions.Smile},
            {":d", ChatEmotions.Smile},
            {";d", ChatEmotions.Smile},
            {":]", ChatEmotions.Smile},
            {";]", ChatEmotions.Smile},
            {"=)", ChatEmotions.Smile},
            {"=]", ChatEmotions.Smile},
            {":-)", ChatEmotions.Smile},

            // Angry
            {">:(", ChatEmotions.Angry},
            {">:[", ChatEmotions.Angry},
            {">;[", ChatEmotions.Angry},
            {">;(", ChatEmotions.Angry},
            {">=(", ChatEmotions.Angry},

            // Shocked
            {":o", ChatEmotions.Shocked},
            {";o", ChatEmotions.Shocked},
            {">;o", ChatEmotions.Shocked},
            {">:o", ChatEmotions.Shocked},
            {"=o", ChatEmotions.Shocked},
            {">=o", ChatEmotions.Shocked},

            // Sad
            {";'(", ChatEmotions.Sad},
            {";[", ChatEmotions.Sad},
            {":[", ChatEmotions.Sad},
            {";(", ChatEmotions.Sad},
            {"=(", ChatEmotions.Sad},
            {"='(", ChatEmotions.Sad},
            {"=[", ChatEmotions.Sad},
            {"='[", ChatEmotions.Sad},
            {":(", ChatEmotions.Sad},
            {":-(", ChatEmotions.Sad}
        };

        public int GetEmotionsForText(string Text)
        {
            foreach (var Kvp in Emotions)
            {
                if (Text.ToLower().Contains(Kvp.Key.ToLower()))
                {
                    return GetEmoticonPacketNum(Kvp.Value);
                }
            }

            return 0;
        }

        private static int GetEmoticonPacketNum(ChatEmotions e)
        {
            switch (e)
            {
                case ChatEmotions.Smile:
                    return 1;
                case ChatEmotions.Angry:
                    return 2;
                case ChatEmotions.Shocked:
                    return 3;
                case ChatEmotions.Sad:
                    return 4;
                case ChatEmotions.None:
                default:
                    return 0;
            }
        }
    }
}