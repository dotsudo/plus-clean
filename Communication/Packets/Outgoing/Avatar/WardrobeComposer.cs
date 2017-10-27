namespace Plus.Communication.Packets.Outgoing.Avatar
{
    using System;
    using System.Data;
    using HabboHotel.GameClients;

    internal class WardrobeComposer : ServerPacket
    {
        public WardrobeComposer(GameClient session)
            : base(ServerPacketHeader.WardrobeMessageComposer)
        {
            WriteInteger(1);
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `slot_id`,`look`,`gender` FROM `user_wardrobe` WHERE `user_id` = '" + session.GetHabbo().Id + "'");
                var wardrobeData = dbClient.GetTable();

                if (wardrobeData == null)
                {
                    WriteInteger(0);
                }
                else
                {
                    WriteInteger(wardrobeData.Rows.Count);
                    foreach (DataRow row in wardrobeData.Rows)
                    {
                        WriteInteger(Convert.ToInt32(row["slot_id"]));
                        WriteString(Convert.ToString(row["look"]));
                        WriteString(row["gender"].ToString().ToUpper());
                    }
                }
            }
        }
    }
}