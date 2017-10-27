﻿namespace Plus.Communication.Packets.Incoming.Messenger
{
    using System.Linq;
    using HabboHotel.GameClients;

    internal class RemoveBuddyEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            var amount = packet.PopInt();
            if (amount > 100)
            {
                amount = 100;
            }
            else if (amount < 0)
            {
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                for (var i = 0; i < amount; i++)
                {
                    var id = packet.PopInt();

                    if (session.GetHabbo().Relationships.Count(x => x.Value.UserId == id) > 0)
                    {
                        dbClient.SetQuery("DELETE FROM `user_relationships` WHERE `user_id` = @id AND `target` = @target OR `target` = @id AND `user_id` = @target");
                        dbClient.AddParameter("id", session.GetHabbo().Id);
                        dbClient.AddParameter("target", id);
                        dbClient.RunQuery();
                    }

                    if (session.GetHabbo().Relationships.ContainsKey(id))
                    {
                        session.GetHabbo().Relationships.Remove(id);
                    }

                    var target = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(id);
                    if (target != null)
                    {
                        if (target.GetHabbo().Relationships.ContainsKey(session.GetHabbo().Id))
                        {
                            target.GetHabbo().Relationships.Remove(session.GetHabbo().Id);
                        }
                    }

                    session.GetHabbo().GetMessenger().DestroyFriendship(id);
                }
            }
        }
    }
}