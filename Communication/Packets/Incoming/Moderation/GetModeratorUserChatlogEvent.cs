namespace Plus.Communication.Packets.Incoming.Moderation
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using HabboHotel.Rooms.Chat.Logs;
    using Outgoing.Moderation;
    using Utilities;

    internal class GetModeratorUserChatlogEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
            {
                return;
            }

            if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            var data = PlusEnvironment.GetHabboById(packet.PopInt());
            if (data == null)
            {
                session.SendNotification("Unable to load info for user.");
                return;
            }

            PlusEnvironment.GetGame().GetChatManager().GetLogs().FlushAndSave();

            var chatlogs = new List<KeyValuePair<RoomData, List<ChatlogEntry>>>();
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `room_id`,`entry_timestamp`,`exit_timestamp` FROM `user_roomvisits` WHERE `user_id` = '" + data.Id +
                                  "' ORDER BY `entry_timestamp` DESC LIMIT 7");
                var getLogs = dbClient.GetTable();

                if (getLogs != null)
                {
                    chatlogs.AddRange(from DataRow row in getLogs.Rows
                        let roomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(Convert.ToInt32(row["room_id"]))
                        where roomData != null
                        let timestampExit = Convert.ToDouble(row["exit_timestamp"]) <= 0 ? UnixTimestamp.GetNow() : Convert.ToDouble(row["exit_timestamp"])
                        select new KeyValuePair<RoomData, List<ChatlogEntry>>(roomData, GetChatlogs(roomData, Convert.ToDouble(row["entry_timestamp"]), timestampExit)));
                }

                session.SendPacket(new ModeratorUserChatlogComposer(data, chatlogs));
            }
        }

        private List<ChatlogEntry> GetChatlogs(RoomData roomData, double timeEnter, double timeExit)
        {
            var chats = new List<ChatlogEntry>();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `user_id`, `timestamp`, `message` FROM `chatlogs` WHERE `room_id` = " + roomData.Id + " AND `timestamp` > " + timeEnter +
                                  " AND `timestamp` < " + timeExit + " ORDER BY `timestamp` DESC LIMIT 100");
                var data = dbClient.GetTable();

                if (data != null)
                {
                    chats.AddRange(from DataRow row in data.Rows
                        let habbo = PlusEnvironment.GetHabboById(Convert.ToInt32(row["user_id"]))
                        where habbo != null
                        select new ChatlogEntry(Convert.ToInt32(row["user_id"]), roomData.Id, Convert.ToString(row["message"]), Convert.ToDouble(row["timestamp"]), habbo));
                }
            }

            return chats;
        }
    }
}