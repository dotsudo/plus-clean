﻿namespace Plus.Communication.Packets.Incoming.Rooms.Avatar
{
    using System;
    using HabboHotel.GameClients;
    using HabboHotel.Quests;
    using Outgoing.Rooms.Engine;
    using Utilities;

    internal class ChangeMottoEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session.GetHabbo().TimeMuted > 0)
            {
                session.SendNotification("Oops, you're currently muted - you cannot change your motto.");
                return;
            }

            if ((DateTime.Now - session.GetHabbo().LastMottoUpdateTime).TotalSeconds <= 2.0)
            {
                session.GetHabbo().MottoUpdateWarnings += 1;
                if (session.GetHabbo().MottoUpdateWarnings >= 25)
                {
                    session.GetHabbo().SessionMottoBlocked = true;
                }
                return;
            }

            if (session.GetHabbo().SessionMottoBlocked)
            {
                return;
            }

            session.GetHabbo().LastMottoUpdateTime = DateTime.Now;

            var newMotto = StringCharFilter.Escape(packet.PopString().Trim());

            if (newMotto.Length > 38)
            {
                newMotto = newMotto.Substring(0, 38);
            }

            if (newMotto == session.GetHabbo().Motto)
            {
                return;
            }

            if (!session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
            {
                newMotto = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(newMotto);
            }

            session.GetHabbo().Motto = newMotto;

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `motto` = @motto WHERE `id` = @userId LIMIT 1");
                dbClient.AddParameter("userId", session.GetHabbo().Id);
                dbClient.AddParameter("motto", newMotto);
                dbClient.RunQuery();
            }

            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.ProfileChangeMotto);
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_Motto", 1);

            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user?.GetClient() == null)
            {
                return;
            }

            room.SendPacket(new UserChangeComposer(user, false));
        }
    }
}