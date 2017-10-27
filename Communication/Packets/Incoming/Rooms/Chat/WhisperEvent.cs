﻿namespace Plus.Communication.Packets.Incoming.Rooms.Chat
{
    using System;
    using HabboHotel.GameClients;
    using HabboHotel.Moderation;
    using HabboHotel.Quests;
    using HabboHotel.Rooms.Chat.Logs;
    using HabboHotel.Rooms.Chat.Styles;
    using Outgoing.Moderation;
    using Outgoing.Rooms.Chat;
    using Utilities;

    public class WhisperEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
            {
                return;
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool") && Room.CheckMute(Session))
            {
                Session.SendWhisper("Oops, you're currently muted.");
                return;
            }

            if (PlusEnvironment.GetUnixTimestamp() < Session.GetHabbo().FloodTime && Session.GetHabbo().FloodTime != 0)
            {
                return;
            }

            var Params = packet.PopString();
            var ToUser = Params.Split(' ')[0];
            var Message = Params.Substring(ToUser.Length + 1);
            var Colour = packet.PopInt();

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
            {
                return;
            }

            var User2 = Room.GetRoomUserManager().GetRoomUserByHabbo(ToUser);
            if (User2 == null)
            {
                return;
            }

            if (Session.GetHabbo().TimeMuted > 0)
            {
                Session.SendPacket(new MutedComposer(Session.GetHabbo().TimeMuted));
                return;
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
            {
                Message = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Message);
            }

            ChatStyle Style = null;
            if (!PlusEnvironment.GetGame().GetChatManager().GetChatStyles().TryGetStyle(Colour, out Style) ||
                Style.RequiredRight.Length > 0 && !Session.GetHabbo().GetPermissions().HasRight(Style.RequiredRight))
            {
                Colour = 0;
            }

            User.LastBubble = Session.GetHabbo().CustomBubbleId == 0 ? Colour : Session.GetHabbo().CustomBubbleId;

            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                int MuteTime;
                if (User.IncrementAndCheckFlood(out MuteTime))
                {
                    Session.SendPacket(new FloodControlComposer(MuteTime));
                    return;
                }
            }

            if (!User2.GetClient().GetHabbo().ReceiveWhispers && !Session.GetHabbo().GetPermissions().HasRight("room_whisper_override"))
            {
                Session.SendWhisper("Oops, this user has their whispers disabled!");
                return;
            }

            PlusEnvironment.GetGame().GetChatManager().GetLogs().StoreChatlog(new ChatlogEntry(Session.GetHabbo().Id, Room.Id, "<Whisper to " + ToUser + ">: " + Message,
                UnixTimestamp.GetNow(), Session.GetHabbo(), Room));

            if (PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckBannedWords(Message))
            {
                Session.GetHabbo().BannedPhraseCount++;
                if (Session.GetHabbo().BannedPhraseCount >= Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("room.chat.filter.banned_phrases.chances")))
                {
                    PlusEnvironment.GetGame().GetModerationManager().BanUser("System", ModerationBanType.Username, Session.GetHabbo().Username,
                        "Spamming banned phrases (" + Message + ")", PlusEnvironment.GetUnixTimestamp() + 78892200);
                    Session.Disconnect();
                    return;
                }

                Session.SendPacket(new WhisperComposer(User.VirtualId, Message, 0, User.LastBubble));
                return;
            }

            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SocialChat);

            User.UnIdle();
            User.GetClient().SendPacket(new WhisperComposer(User.VirtualId, Message, 0, User.LastBubble));

            if (User2 != null && !User2.IsBot && User2.UserId != User.UserId)
            {
                if (!User2.GetClient().GetHabbo().GetIgnores().IgnoredUserIds().Contains(Session.GetHabbo().Id))
                {
                    User2.GetClient().SendPacket(new WhisperComposer(User.VirtualId, Message, 0, User.LastBubble));
                }
            }

            var ToNotify = Room.GetRoomUserManager().GetRoomUserByRank(2);
            if (ToNotify.Count > 0)
            {
                foreach (var user in ToNotify)
                {
                    if (user != null && user.HabboId != User2.HabboId && user.HabboId != User.HabboId)
                    {
                        if (user.GetClient() != null && user.GetClient().GetHabbo() != null && !user.GetClient().GetHabbo().IgnorePublicWhispers)
                        {
                            user.GetClient().SendPacket(new WhisperComposer(User.VirtualId, "[Whisper to " + ToUser + "] " + Message, 0, User.LastBubble));
                        }
                    }
                }
            }
        }
    }
}