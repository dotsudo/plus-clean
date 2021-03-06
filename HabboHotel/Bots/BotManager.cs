﻿namespace Plus.HabboHotel.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using log4net;
    using Rooms.AI;
    using Rooms.AI.Responses;

    public class BotManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Bots.BotManager");

        private readonly List<BotResponse> _responses;

        public BotManager() => _responses = new List<BotResponse>();

        public void Init()
        {
            if (_responses.Count > 0)
            {
                _responses.Clear();
            }
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `bot_ai`,`chat_keywords`,`response_text`,`response_mode`,`response_beverage` FROM `bots_responses`");
                var BotResponses = dbClient.GetTable();
                if (BotResponses != null)
                {
                    foreach (DataRow Response in BotResponses.Rows)
                    {
                        _responses.Add(new BotResponse(Convert.ToString(Response["bot_ai"]),
                            Convert.ToString(Response["chat_keywords"]),
                            Convert.ToString(Response["response_text"]),
                            Response["response_mode"].ToString(),
                            Convert.ToString(Response["response_beverage"])));
                    }
                }
            }
        }

        public BotResponse GetResponse(BotAIType AiType, string Message)
        {
            foreach (var Response in _responses.Where(X => X.AiType == AiType).ToList())
            {
                if (Response.KeywordMatched(Message))
                {
                    return Response;
                }
            }

            return null;
        }
    }
}