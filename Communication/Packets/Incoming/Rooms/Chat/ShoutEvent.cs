namespace Plus.Communication.Packets.Incoming.Rooms.Chat
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

    public class ShoutEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || !session.GetHabbo().InRoom)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
            {
                return;
            }

            var message = StringCharFilter.Escape(packet.PopString());
            if (message.Length > 100)
            {
                message = message.Substring(0, 100);
            }

            var colour = packet.PopInt();

            ChatStyle style;
            if (!PlusEnvironment.GetGame().GetChatManager().GetChatStyles().TryGetStyle(colour, out style) ||
                style.RequiredRight.Length > 0 && !session.GetHabbo().GetPermissions().HasRight(style.RequiredRight))
            {
                colour = 0;
            }

            user.LastBubble = session.GetHabbo().CustomBubbleId == 0 ? colour : session.GetHabbo().CustomBubbleId;

            if (PlusEnvironment.GetUnixTimestamp() < session.GetHabbo().FloodTime && session.GetHabbo().FloodTime != 0)
            {
                return;
            }

            if (session.GetHabbo().TimeMuted > 0)
            {
                session.SendPacket(new MutedComposer(session.GetHabbo().TimeMuted));
                return;
            }

            if (!session.GetHabbo().GetPermissions().HasRight("room_ignore_mute") && room.CheckMute(session))
            {
                session.SendWhisper("Oops, you're currently muted.");
                return;
            }

            if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                int muteTime;
                if (user.IncrementAndCheckFlood(out muteTime))
                {
                    session.SendPacket(new FloodControlComposer(muteTime));
                    return;
                }
            }

            PlusEnvironment.GetGame().GetChatManager().GetLogs()
                .StoreChatlog(new ChatlogEntry(session.GetHabbo().Id, room.Id, message, UnixTimestamp.GetNow(), session.GetHabbo(), room));

            if (message.StartsWith(":", StringComparison.CurrentCulture) && PlusEnvironment.GetGame().GetChatManager().GetCommands().Parse(session, message))
            {
                return;
            }

            if (PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckBannedWords(message))
            {
                session.GetHabbo().BannedPhraseCount++;
                if (session.GetHabbo().BannedPhraseCount >= Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("room.chat.filter.banned_phrases.chances")))
                {
                    PlusEnvironment.GetGame().GetModerationManager().BanUser("System", ModerationBanType.Username, session.GetHabbo().Username,
                        "Spamming banned phrases (" + message + ")", PlusEnvironment.GetUnixTimestamp() + 78892200);
                    session.Disconnect();
                    return;
                }

                session.SendPacket(new ShoutComposer(user.VirtualId, message, 0, colour));
                return;
            }

            if (!session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
            {
                message = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(message);
            }

            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.SocialChat);

            user.UnIdle();
            user.OnChat(user.LastBubble, message, true);
        }
    }
}