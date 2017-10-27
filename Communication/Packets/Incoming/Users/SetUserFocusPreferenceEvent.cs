﻿namespace Plus.Communication.Packets.Incoming.Users
{
    using HabboHotel.GameClients;

    internal class SetUserFocusPreferenceEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var focusPreference = packet.PopBoolean();

            session.GetHabbo().FocusPreference = focusPreference;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `focus_preference` = @focusPreference WHERE `id` = '" + session.GetHabbo().Id + "' LIMIT 1");
                dbClient.AddParameter("focusPreference", PlusEnvironment.BoolToEnum(focusPreference));
                dbClient.RunQuery();
            }
        }
    }
}