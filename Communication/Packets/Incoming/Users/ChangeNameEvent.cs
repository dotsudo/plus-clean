namespace Plus.Communication.Packets.Incoming.Users
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using HabboHotel.Users;
    using Outgoing.Navigator;
    using Outgoing.Rooms.Engine;
    using Outgoing.Rooms.Session;
    using Outgoing.Users;

    internal class ChangeNameEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;

            var user = room?.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Username);
            if (user == null)
            {
                return;
            }

            var newName = packet.PopString();
            var oldName = session.GetHabbo().Username;

            if (newName == oldName)
            {
                session.GetHabbo().ChangeName(oldName);
                session.SendPacket(new UpdateUsernameComposer(newName));
                return;
            }

            if (!CanChangeName(session.GetHabbo()))
            {
                session.SendNotification("Oops, it appears you currently cannot change your username!");
                return;
            }

            bool inUse;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT COUNT(0) FROM `users` WHERE `username` = @name LIMIT 1");
                dbClient.AddParameter("name", newName);
                inUse = dbClient.GetInteger() == 1;
            }

            var letters = newName.ToLower().ToCharArray();
            var allowedCharacters = "abcdefghijklmnopqrstuvwxyz.,_-;:?!1234567890";

            foreach (var chr in letters)
            {
                if (!allowedCharacters.Contains(chr))
                {
                    return;
                }
            }

            if (!session.GetHabbo().GetPermissions().HasRight("mod_tool") && newName.ToLower().Contains("mod") || newName.ToLower().Contains("adm") ||
                newName.ToLower().Contains("admin")
                || newName.ToLower().Contains("m0d") || newName.ToLower().Contains("mob") || newName.ToLower().Contains("m0b"))
            {
                return;
            }

            if (!newName.ToLower().Contains("mod") && (session.GetHabbo().Rank == 2 || session.GetHabbo().Rank == 3))
            {
            }
            else if (newName.Length > 15)
            {
            }
            else if (newName.Length < 3)
            {
            }
            else if (inUse)
            {
            }
            else
            {
                if (!PlusEnvironment.GetGame().GetClientManager().UpdateClientUsername(session, oldName, newName))
                {
                    session.SendNotification("Oops! An issue occoured whilst updating your username.");
                    return;
                }

                session.GetHabbo().ChangingName = false;

                room.GetRoomUserManager().RemoveUserFromRoom(session, true);

                session.GetHabbo().ChangeName(newName);
                session.GetHabbo().GetMessenger().OnStatusChanged(true);

                session.SendPacket(new UpdateUsernameComposer(newName));
                room.SendPacket(new UserNameChangeComposer(room.Id, user.VirtualId, newName));

                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("INSERT INTO `logs_client_namechange` (`user_id`,`new_name`,`old_name`,`timestamp`) VALUES ('" + session.GetHabbo().Id + "', @name, '" +
                                      oldName + "', '" + PlusEnvironment.GetUnixTimestamp() + "')");
                    dbClient.AddParameter("name", newName);
                    dbClient.RunQuery();
                }

                ICollection<RoomData> rooms = session.GetHabbo().UsersRooms;
                foreach (var data in rooms)
                {
                    if (data == null)
                    {
                        continue;
                    }

                    data.OwnerName = newName;
                }

                foreach (var userRoom in PlusEnvironment.GetGame().GetRoomManager().GetRooms().ToList())
                {
                    if (userRoom == null || userRoom.RoomData.OwnerName != newName)
                    {
                        continue;
                    }

                    userRoom.OwnerName = newName;
                    userRoom.RoomData.OwnerName = newName;

                    userRoom.SendPacket(new RoomInfoUpdatedComposer(userRoom.RoomId));
                }

                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_Name", 1);

                session.SendPacket(new RoomForwardComposer(room.Id));
            }
        }

        private static bool CanChangeName(Habbo habbo)
        {
            if (habbo.Rank == 1 && habbo.VipRank == 0 && habbo.LastNameChange == 0)
            {
                return true;
            }

            if (habbo.Rank == 1 && habbo.VipRank == 1 && (habbo.LastNameChange == 0 || PlusEnvironment.GetUnixTimestamp() + 604800 > habbo.LastNameChange))
            {
                return true;
            }

            if (habbo.Rank == 1 && habbo.VipRank == 2 && (habbo.LastNameChange == 0 || PlusEnvironment.GetUnixTimestamp() + 86400 > habbo.LastNameChange))
            {
                return true;
            }

            if (habbo.Rank == 1 && habbo.VipRank == 3)
            {
                return true;
            }

            if (habbo.GetPermissions().HasRight("mod_tool"))
            {
                return true;
            }

            return false;
        }
    }
}