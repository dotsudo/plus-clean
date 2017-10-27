namespace Plus.Communication.Packets.Incoming.Rooms.AI.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using HabboHotel.Rooms.AI;
    using HabboHotel.Rooms.AI.Speech;
    using HabboHotel.Users.Inventory.Bots;
    using Outgoing.Inventory.Bots;

    internal class PlaceBotEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            Room room;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out room))
            {
                return;
            }

            if (!room.CheckRights(session, true))
            {
                return;
            }

            var botId = packet.PopInt();
            var x = packet.PopInt();
            var y = packet.PopInt();

            if (!room.GetGameMap().CanWalk(x, y, false) || !room.GetGameMap().ValidTile(x, y))
            {
                session.SendNotification("You cannot place a bot here!");
                return;
            }

            Bot bot;
            if (!session.GetHabbo().GetInventoryComponent().TryGetBot(botId, out bot))
            {
                return;
            }

            var botCount = 0;
            foreach (var user in room.GetRoomUserManager().GetUserList().ToList())
            {
                if (user == null || user.IsPet || !user.IsBot)
                {
                    continue;
                }

                botCount += 1;
            }

            if (botCount >= 5 && !session.GetHabbo().GetPermissions().HasRight("bot_place_any_override"))
            {
                session.SendNotification("Sorry; 5 bots per room only!");
                return;
            }

            //TODO: Hmm, maybe not????
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `bots` SET `room_id` = @roomId, `x` = @CoordX, `y` = @CoordY WHERE `id` = @BotId LIMIT 1");
                dbClient.AddParameter("roomId", room.RoomId);
                dbClient.AddParameter("BotId", bot.Id);
                dbClient.AddParameter("CoordX", x);
                dbClient.AddParameter("CoordY", y);
                dbClient.RunQuery();
            }

            var botSpeechList = new List<RandomSpeech>();

            //TODO: Grab data?
            DataRow getData = null;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `ai_type`,`rotation`,`walk_mode`,`automatic_chat`,`speaking_interval`,`mix_sentences`,`chat_bubble` FROM `bots` WHERE `id` = @BotId LIMIT 1");
                dbClient.AddParameter("BotId", bot.Id);
                getData = dbClient.GetRow();

                dbClient.SetQuery("SELECT `text` FROM `bots_speech` WHERE `bot_id` = @BotId");
                dbClient.AddParameter("BotId", bot.Id);
                var botSpeech = dbClient.GetTable();

                foreach (DataRow speech in botSpeech.Rows)
                {
                    botSpeechList.Add(new RandomSpeech(Convert.ToString(speech["text"]), bot.Id));
                }
            }

            var botUser = room.GetRoomUserManager().DeployBot(
                new RoomBot(bot.Id, session.GetHabbo().CurrentRoomId, Convert.ToString(getData["ai_type"]), Convert.ToString(getData["walk_mode"]), bot.Name, "", bot.Figure, x, y,
                    0, 4, 0, 0, 0, 0, ref botSpeechList, "", 0, bot.OwnerId, PlusEnvironment.EnumToBool(getData["automatic_chat"].ToString()),
                    Convert.ToInt32(getData["speaking_interval"]), PlusEnvironment.EnumToBool(getData["mix_sentences"].ToString()), Convert.ToInt32(getData["chat_bubble"])), null);
            botUser.Chat("Hello!", false);

            room.GetGameMap().UpdateUserMovement(new Point(x, y), new Point(x, y), botUser);

            Bot toRemove = null;
            if (!session.GetHabbo().GetInventoryComponent().TryRemoveBot(botId, out toRemove))
            {
                Console.WriteLine("Error whilst removing Bot: " + toRemove.Id);
                return;
            }

            session.SendPacket(new BotInventoryComposer(session.GetHabbo().GetInventoryComponent().GetBots()));
        }
    }
}