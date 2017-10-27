namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using HabboHotel.GameClients;
    using HabboHotel.Navigator;
    using HabboHotel.Rooms;
    using Outgoing.Navigator;
    using Outgoing.Rooms.Engine;
    using Outgoing.Rooms.Settings;

    internal class SaveRoomSettingsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
            {
                return;
            }

            var room = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(packet.PopInt());
            if (room == null || !room.CheckRights(session, true))
            {
                return;
            }

            var name = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(packet.PopString());
            var description = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(packet.PopString());
            var access = RoomAccessUtility.ToRoomAccess(packet.PopInt());
            var password = packet.PopString();
            var maxUsers = packet.PopInt();
            var categoryId = packet.PopInt();
            var tagCount = packet.PopInt();

            var tags = new List<string>();
            var formattedTags = new StringBuilder();

            for (var i = 0; i < tagCount; i++)
            {
                if (i > 0)
                {
                    formattedTags.Append(",");
                }

                var tag = packet.PopString().ToLower();

                tags.Add(tag);
                formattedTags.Append(tag);
            }

            var tradeSettings = packet.PopInt(); //2 = All can trade, 1 = owner only, 0 = no trading.
            var allowPets = Convert.ToInt32(PlusEnvironment.BoolToEnum(packet.PopBoolean()));
            var allowPetsEat = Convert.ToInt32(PlusEnvironment.BoolToEnum(packet.PopBoolean()));
            var roomBlockingEnabled = Convert.ToInt32(PlusEnvironment.BoolToEnum(packet.PopBoolean()));
            var hidewall = Convert.ToInt32(PlusEnvironment.BoolToEnum(packet.PopBoolean()));
            var wallThickness = packet.PopInt();
            var floorThickness = packet.PopInt();
            var whoMute = packet.PopInt(); // mute
            var whoKick = packet.PopInt(); // kick
            var whoBan = packet.PopInt(); // ban

            var chatMode = packet.PopInt();
            var chatSize = packet.PopInt();
            var chatSpeed = packet.PopInt();
            var chatDistance = packet.PopInt();
            var extraFlood = packet.PopInt();

            if (chatMode < 0 || chatMode > 1)
            {
                chatMode = 0;
            }

            if (chatSize < 0 || chatSize > 2)
            {
                chatSize = 0;
            }

            if (chatSpeed < 0 || chatSpeed > 2)
            {
                chatSpeed = 0;
            }

            if (chatDistance < 0)
            {
                chatDistance = 1;
            }

            if (chatDistance > 99)
            {
                chatDistance = 100;
            }

            if (extraFlood < 0 || extraFlood > 2)
            {
                extraFlood = 0;
            }

            if (tradeSettings < 0 || tradeSettings > 2)
            {
                tradeSettings = 0;
            }

            if (whoMute < 0 || whoMute > 1)
            {
                whoMute = 0;
            }

            if (whoKick < 0 || whoKick > 1)
            {
                whoKick = 0;
            }

            if (whoBan < 0 || whoBan > 1)
            {
                whoBan = 0;
            }

            if (wallThickness < -2 || wallThickness > 1)
            {
                wallThickness = 0;
            }

            if (floorThickness < -2 || floorThickness > 1)
            {
                floorThickness = 0;
            }

            if (name.Length < 1)
            {
                return;
            }

            if (name.Length > 60)
            {
                name = name.Substring(0, 60);
            }

            if (access == RoomAccess.PASSWORD && password.Length == 0)
            {
                access = RoomAccess.OPEN;
            }

            if (maxUsers < 0)
            {
                maxUsers = 10;
            }

            if (maxUsers > 50)
            {
                maxUsers = 50;
            }

            SearchResultList searchResultList = null;
            if (!PlusEnvironment.GetGame().GetNavigator().TryGetSearchResultList(categoryId, out searchResultList))
            {
                categoryId = 36;
            }

            if (searchResultList.CategoryType != NavigatorCategoryType.CATEGORY || searchResultList.RequiredRank > session.GetHabbo().Rank ||
                session.GetHabbo().Id != room.OwnerId && session.GetHabbo().Rank >= searchResultList.RequiredRank)
            {
                categoryId = 36;
            }

            if (tagCount > 2)
            {
                return;
            }

            room.AllowPets = allowPets;
            room.AllowPetsEating = allowPetsEat;
            room.RoomBlockingEnabled = roomBlockingEnabled;
            room.Hidewall = hidewall;

            room.RoomData.AllowPets = allowPets;
            room.RoomData.AllowPetsEating = allowPetsEat;
            room.RoomData.RoomBlockingEnabled = roomBlockingEnabled;
            room.RoomData.Hidewall = hidewall;

            room.Name = name;
            room.Access = access;
            room.Description = description;
            room.Category = categoryId;
            room.Password = password;

            room.RoomData.Name = name;
            room.RoomData.Access = access;
            room.RoomData.Description = description;
            room.RoomData.Category = categoryId;
            room.RoomData.Password = password;

            room.WhoCanBan = whoBan;
            room.WhoCanKick = whoKick;
            room.WhoCanMute = whoMute;
            room.RoomData.WhoCanBan = whoBan;
            room.RoomData.WhoCanKick = whoKick;
            room.RoomData.WhoCanMute = whoMute;

            room.ClearTags();
            room.AddTagRange(tags);
            room.UsersMax = maxUsers;

            room.RoomData.Tags.Clear();
            room.RoomData.Tags.AddRange(tags);
            room.RoomData.UsersMax = maxUsers;

            room.WallThickness = wallThickness;
            room.FloorThickness = floorThickness;
            room.RoomData.WallThickness = wallThickness;
            room.RoomData.FloorThickness = floorThickness;

            room.chatMode = chatMode;
            room.chatSize = chatSize;
            room.chatSpeed = chatSpeed;
            room.chatDistance = chatDistance;
            room.extraFlood = extraFlood;

            room.TradeSettings = tradeSettings;

            room.RoomData.chatMode = chatMode;
            room.RoomData.chatSize = chatSize;
            room.RoomData.chatSpeed = chatSpeed;
            room.RoomData.chatDistance = chatDistance;
            room.RoomData.extraFlood = extraFlood;

            room.RoomData.TradeSettings = tradeSettings;

            var accessStr = password.Length > 0 ? "password" : "open";
            switch (access)
            {
                default:
                case RoomAccess.OPEN:
                    accessStr = "open";
                    break;

                case RoomAccess.PASSWORD:
                    accessStr = "password";
                    break;

                case RoomAccess.DOORBELL:
                    accessStr = "locked";
                    break;

                case RoomAccess.INVISIBLE:
                    accessStr = "invisible";
                    break;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "UPDATE `rooms` SET `caption` = @caption, `description` = @description, `password` = @password, `category` = @categoryId, `state` = @state, `tags` = @tags, `users_max` = @maxUsers, `allow_pets` = @allowPets, `allow_pets_eat` = @allowPetsEat, `room_blocking_disabled` = @roomBlockingDisabled, `allow_hidewall` = @allowHidewall, `floorthick` = @floorThick, `wallthick` = @wallThick, `mute_settings` = @muteSettings, `kick_settings` = @kickSettings, `ban_settings` = @banSettings, `chat_mode` = @chatMode, `chat_size` = @chatSize, `chat_speed` = @chatSpeed, `chat_extra_flood` = @extraFlood, `chat_hearing_distance` = @chatDistance, `trade_settings` = @tradeSettings WHERE `id` = @roomId LIMIT 1");
                dbClient.AddParameter("categoryId", categoryId);
                dbClient.AddParameter("maxUsers", maxUsers);
                dbClient.AddParameter("allowPets", allowPets);
                dbClient.AddParameter("allowPetsEat", allowPetsEat);
                dbClient.AddParameter("roomBlockingDisabled", roomBlockingEnabled);
                dbClient.AddParameter("allowHidewall", room.Hidewall);
                dbClient.AddParameter("floorThick", room.FloorThickness);
                dbClient.AddParameter("wallThick", room.WallThickness);
                dbClient.AddParameter("muteSettings", room.WhoCanMute);
                dbClient.AddParameter("kickSettings", room.WhoCanKick);
                dbClient.AddParameter("banSettings", room.WhoCanBan);
                dbClient.AddParameter("chatMode", room.chatMode);
                dbClient.AddParameter("chatSize", room.chatSize);
                dbClient.AddParameter("chatSpeed", room.chatSpeed);
                dbClient.AddParameter("extraFlood", room.extraFlood);
                dbClient.AddParameter("chatDistance", room.chatDistance);
                dbClient.AddParameter("tradeSettings", room.TradeSettings);
                dbClient.AddParameter("roomId", room.Id);
                dbClient.AddParameter("caption", room.Name);
                dbClient.AddParameter("description", room.Description);
                dbClient.AddParameter("password", room.Password);
                dbClient.AddParameter("state", accessStr);
                dbClient.AddParameter("tags", formattedTags.ToString());
                dbClient.RunQuery();
            }

            room.GetGameMap().GenerateMaps();

            if (session.GetHabbo().CurrentRoom == null)
            {
                session.SendPacket(new RoomSettingsSavedComposer(room.RoomId));
                session.SendPacket(new RoomInfoUpdatedComposer(room.RoomId));
                session.SendPacket(new RoomVisualizationSettingsComposer(room.WallThickness, room.FloorThickness, PlusEnvironment.EnumToBool(room.Hidewall.ToString())));
            }
            else
            {
                room.SendPacket(new RoomSettingsSavedComposer(room.RoomId));
                room.SendPacket(new RoomInfoUpdatedComposer(room.RoomId));
                room.SendPacket(new RoomVisualizationSettingsComposer(room.WallThickness, room.FloorThickness, PlusEnvironment.EnumToBool(room.Hidewall.ToString())));
            }

            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_SelfModDoorModeSeen", 1);
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_SelfModWalkthroughSeen", 1);
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_SelfModChatScrollSpeedSeen", 1);
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_SelfModChatFloodFilterSeen", 1);
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_SelfModChatHearRangeSeen", 1);
        }
    }
}