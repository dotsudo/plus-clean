namespace Plus.HabboHotel.Rooms.AI
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using Catalog.Utilities;
    using Speech;
    using Types;

    public class RoomBot
    {
        internal BotAIType AiType;

        internal bool AutomaticChat;
        internal int BotId;

        internal int DanceId;
        internal string Gender;
        internal int Id;

        internal string Look;
        public int MaxX;
        public int MaxY;
        public int MinX;
        public int MinY;
        internal bool MixSentences;
        internal string Motto;
        public string Name;

        internal int OwnerId;
        internal List<RandomSpeech> RandomSpeech;
        public int RoomId;

        public RoomUser RoomUser;
        public int Rot;
        public int SpeakingInterval;
        public int VirtualId;

        internal string WalkingMode;

        internal int X;
        internal int Y;
        internal double Z;

        internal RoomBot(int botId,
                         int roomId,
                         string aiType,
                         string walkingMode,
                         string name,
                         string motto,
                         string look,
                         int x,
                         int y,
                         double z,
                         int rot,
                         int minX,
                         int minY,
                         int maxX,
                         int maxY,
                         ref List<RandomSpeech> speeches,
                         string gender,
                         int dance,
                         int ownerId,
                         bool automaticChat,
                         int speakingInterval,
                         bool mixSentences,
                         int chatBubble)
        {
            Id = botId;
            BotId = botId;
            RoomId = roomId;
            Name = name;
            Motto = motto;
            Look = look;
            Gender = gender.ToUpper();
            AiType = BotUtility.GetAIFromString(aiType);
            WalkingMode = walkingMode;
            X = x;
            Y = y;
            Z = z;
            Rot = rot;
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
            VirtualId = -1;
            RoomUser = null;
            DanceId = dance;
            LoadRandomSpeech(speeches);
            OwnerId = ownerId;
            AutomaticChat = automaticChat;
            SpeakingInterval = speakingInterval;
            MixSentences = mixSentences;
            ChatBubble = chatBubble;
            ForcedMovement = false;
            TargetCoordinate = new Point();
            TargetUser = 0;
        }

        public bool ForcedMovement { get; set; }
        public int ForcedUserTargetMovement { get; set; }
        public Point TargetCoordinate { get; set; }

        public int TargetUser { get; set; }

        public bool IsPet => AiType == BotAIType.PET;

        public int ChatBubble { get; set; }

        public void LoadRandomSpeech(List<RandomSpeech> speeches)
        {
            RandomSpeech = new List<RandomSpeech>();
            foreach (var speech in speeches)
            {
                if (speech.BotID == BotId)
                {
                    RandomSpeech.Add(speech);
                }
            }
        }

        public RandomSpeech GetRandomSpeech()
        {
            var rand = new Random();
            if (RandomSpeech.Count < 1)
            {
                return new RandomSpeech("", 0);
            }

            return RandomSpeech[rand.Next(0, RandomSpeech.Count - 1)];
        }

        public BotAI GenerateBotAi(int virtualId)
        {
            switch (AiType)
            {
                case BotAIType.PET:
                    return new PetBot(virtualId);
                case BotAIType.GENERIC:
                    return new GenericBot(virtualId);
                case BotAIType.BARTENDER:
                    return new BartenderBot(virtualId);
                default:
                    return new GenericBot(virtualId);
            }
        }
    }
}