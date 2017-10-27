namespace Plus.Communication.Packets.Incoming.Moderation
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using Outgoing.Moderation;

    internal class GetModeratorUserRoomVisitsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            var userId = packet.PopInt();
            var target = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(userId);
            if (target == null)
            {
                return;
            }

            DataTable table;
            var visits = new Dictionary<double, RoomData>();
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `room_id`, `entry_timestamp` FROM `user_roomvisits` WHERE `user_id` = @id ORDER BY `entry_timestamp` DESC LIMIT 50");
                dbClient.AddParameter("id", userId);
                table = dbClient.GetTable();

                if (table != null)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        var rData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(Convert.ToInt32(row["room_id"]));
                        if (rData == null)
                        {
                            return;
                        }

                        if (!visits.ContainsKey(Convert.ToDouble(row["entry_timestamp"])))
                        {
                            visits.Add(Convert.ToDouble(row["entry_timestamp"]), rData);
                        }
                    }
                }
            }

            session.SendPacket(new ModeratorUserRoomVisitsComposer(target.GetHabbo(), visits));
        }
    }
}