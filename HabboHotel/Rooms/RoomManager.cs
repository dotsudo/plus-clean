﻿namespace Plus.HabboHotel.Rooms
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Core;
    using GameClients;
    using log4net;

    public class RoomManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Rooms.RoomManager");
        private readonly ConcurrentDictionary<int, RoomData> _loadedRoomData;

        private readonly Dictionary<string, RoomModel> _roomModels;

        private readonly ConcurrentDictionary<int, Room> _rooms;

        private DateTime _cycleLastExecution;

        public RoomManager()
        {
            _roomModels = new Dictionary<string, RoomModel>();
            _rooms = new ConcurrentDictionary<int, Room>();
            _loadedRoomData = new ConcurrentDictionary<int, RoomData>();
            LoadModels();
            log.Info("Room Manager -> LOADED");
        }

        public int LoadedRoomDataCount => _loadedRoomData.Count;

        public int Count => _rooms.Count;

        public void OnCycle()
        {
            try
            {
                var sinceLastTime = DateTime.Now - _cycleLastExecution;
                if (sinceLastTime.TotalMilliseconds >= 500)
                {
                    _cycleLastExecution = DateTime.Now;
                    foreach (var Room in _rooms.Values.ToList())
                    {
                        if (Room.isCrashed)
                        {
                            continue;
                        }

                        if (Room.ProcessTask == null || Room.ProcessTask.IsCompleted)
                        {
                            Room.ProcessTask = new Task(Room.ProcessRoom);
                            Room.ProcessTask.Start();
                            Room.IsLagging = 0;
                        }
                        else
                        {
                            Room.IsLagging++;
                            if (Room.IsLagging >= 30)
                            {
                                Room.isCrashed = true;
                                UnloadRoom(Room);

                                // Logging.WriteLine("[RoomMgr] Room crashed (task didn't complete within 30 seconds): " + Room.RoomId);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        public void LoadModels()
        {
            if (_roomModels.Count > 0)
            {
                _roomModels.Clear();
            }
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT id,door_x,door_y,door_z,door_dir,heightmap,public_items,club_only,poolmap,`wall_height` FROM `room_models` WHERE `custom` = '0'");
                var Data = dbClient.GetTable();
                if (Data == null)
                {
                    return;
                }

                foreach (DataRow Row in Data.Rows)
                {
                    var Modelname = Convert.ToString(Row["id"]);
                    var staticFurniture = Convert.ToString(Row["public_items"]);
                    _roomModels.Add(Modelname,
                        new RoomModel(Convert.ToInt32(Row["door_x"]),
                            Convert.ToInt32(Row["door_y"]),
                            (double) Row["door_z"],
                            Convert.ToInt32(Row["door_dir"]),
                            Convert.ToString(Row["heightmap"]),
                            Convert.ToString(Row["public_items"]),
                            PlusEnvironment.EnumToBool(Row["club_only"].ToString()),
                            Convert.ToString(Row["poolmap"]),
                            Convert.ToInt32(Row["wall_height"])));
                }
            }
        }

        public void LoadModel(string Id)
        {
            DataRow Row = null;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT id,door_x,door_y,door_z,door_dir,heightmap,public_items,club_only,poolmap,`wall_height` FROM `room_models` WHERE `custom` = '1' AND `id` = '" +
                    Id +
                    "' LIMIT 1");
                Row = dbClient.GetRow();
                if (Row == null)
                {
                    return;
                }

                var Modelname = Convert.ToString(Row["id"]);
                if (!_roomModels.ContainsKey(Id))
                {
                    _roomModels.Add(Modelname,
                        new RoomModel(Convert.ToInt32(Row["door_x"]),
                            Convert.ToInt32(Row["door_y"]),
                            Convert.ToDouble(Row["door_z"]),
                            Convert.ToInt32(Row["door_dir"]),
                            Convert.ToString(Row["heightmap"]),
                            Convert.ToString(Row["public_items"]),
                            PlusEnvironment.EnumToBool(Row["club_only"].ToString()),
                            Convert.ToString(Row["poolmap"]),
                            Convert.ToInt32(Row["wall_height"])));
                }
            }
        }

        public void ReloadModel(string Id)
        {
            if (!_roomModels.ContainsKey(Id))
            {
                LoadModel(Id);
                return;
            }

            _roomModels.Remove(Id);
            LoadModel(Id);
        }

        public bool TryGetModel(string Id, out RoomModel Model) => _roomModels.TryGetValue(Id, out Model);

        public void UnloadRoom(Room Room, bool RemoveData = false)
        {
            if (Room == null)
            {
                return;
            }

            Room room = null;
            if (_rooms.TryRemove(Room.RoomId, out room))
            {
                Room.Dispose();
                if (RemoveData)
                {
                    RoomData Data = null;
                    _loadedRoomData.TryRemove(Room.Id, out Data);
                }
            }

            //Logging.WriteLine("[RoomMgr] Unloaded room: \"" + Room.Name + "\" (ID: " + Room.RoomId + ")");
        }

        public List<RoomData> SearchGroupRooms(string Query)
        {
            var InstanceMatches = (from RoomInstance in _loadedRoomData
                where RoomInstance.Value.UsersNow >= 0 &&
                      RoomInstance.Value.Access != RoomAccess.INVISIBLE &&
                      RoomInstance.Value.Group != null &&
                      (RoomInstance.Value.OwnerName.StartsWith(Query) ||
                       RoomInstance.Value.Tags.Contains(Query) ||
                       RoomInstance.Value.Name.Contains(Query))
                orderby RoomInstance.Value.UsersNow descending
                select RoomInstance.Value).Take(50);
            return InstanceMatches.ToList();
        }

        public List<RoomData> SearchTaggedRooms(string Query)
        {
            var InstanceMatches = (from RoomInstance in _loadedRoomData
                where RoomInstance.Value.UsersNow >= 0 && RoomInstance.Value.Access != RoomAccess.INVISIBLE &&
                      RoomInstance.Value.Tags.Contains(Query)
                orderby RoomInstance.Value.UsersNow descending
                select RoomInstance.Value).Take(50);
            return InstanceMatches.ToList();
        }

        public List<RoomData> GetPopularRooms(int category, int Amount = 50)
        {
            var rooms = (from RoomInstance in _loadedRoomData
                where RoomInstance.Value.UsersNow > 0 &&
                      (category == -1 || RoomInstance.Value.Category == category) &&
                      RoomInstance.Value.Access != RoomAccess.INVISIBLE
                orderby RoomInstance.Value.Score descending
                orderby RoomInstance.Value.UsersNow descending
                select RoomInstance.Value).Take(Amount);
            return rooms.ToList();
        }

        public List<RoomData> GetRecommendedRooms(int Amount = 50, int CurrentRoomId = 0)
        {
            var Rooms = (from RoomInstance in _loadedRoomData
                where RoomInstance.Value.UsersNow >= 0 &&
                      RoomInstance.Value.Score >= 0 &&
                      RoomInstance.Value.Access != RoomAccess.INVISIBLE &&
                      RoomInstance.Value.Id != CurrentRoomId
                orderby RoomInstance.Value.Score descending
                orderby RoomInstance.Value.UsersNow descending
                select RoomInstance.Value).Take(Amount);
            return Rooms.ToList();
        }

        public List<RoomData> GetPopularRatedRooms(int Amount = 50)
        {
            var rooms = (from RoomInstance in _loadedRoomData
                where RoomInstance.Value.Access != RoomAccess.INVISIBLE
                orderby RoomInstance.Value.Score descending
                select RoomInstance.Value).Take(Amount);
            return rooms.ToList();
        }

        public List<RoomData> GetRoomsByCategory(int Category, int Amount = 50)
        {
            var rooms = (from RoomInstance in _loadedRoomData
                where RoomInstance.Value.Category == Category && RoomInstance.Value.UsersNow > 0 &&
                      RoomInstance.Value.Access != RoomAccess.INVISIBLE
                orderby RoomInstance.Value.UsersNow descending
                select RoomInstance.Value).Take(Amount);
            return rooms.ToList();
        }

        public List<RoomData> GetOnGoingRoomPromotions(int Mode, int Amount = 50)
        {
            IEnumerable<RoomData> Rooms = null;
            if (Mode == 17)
            {
                Rooms = (from RoomInstance in _loadedRoomData
                    where RoomInstance.Value.HasActivePromotion && RoomInstance.Value.Access != RoomAccess.INVISIBLE
                    orderby RoomInstance.Value.Promotion.TimestampStarted descending
                    select RoomInstance.Value).Take(Amount);
            }
            else
            {
                Rooms = (from RoomInstance in _loadedRoomData
                    where RoomInstance.Value.HasActivePromotion && RoomInstance.Value.Access != RoomAccess.INVISIBLE
                    orderby RoomInstance.Value.UsersNow descending
                    select RoomInstance.Value).Take(Amount);
            }
            return Rooms.ToList();
        }

        public List<RoomData> GetPromotedRooms(int CategoryId, int Amount = 50)
        {
            IEnumerable<RoomData> Rooms = null;
            Rooms = (from RoomInstance in _loadedRoomData
                where RoomInstance.Value.HasActivePromotion &&
                      RoomInstance.Value.Promotion.CategoryId == CategoryId &&
                      RoomInstance.Value.Access != RoomAccess.INVISIBLE
                orderby RoomInstance.Value.Promotion.TimestampStarted descending
                select RoomInstance.Value).Take(Amount);
            return Rooms.ToList();
        }

        public List<KeyValuePair<string, int>> GetPopularRoomTags()
        {
            var Tags = (from RoomInstance in _loadedRoomData
                where RoomInstance.Value.UsersNow >= 0 && RoomInstance.Value.Access != RoomAccess.INVISIBLE
                orderby RoomInstance.Value.UsersNow descending
                orderby RoomInstance.Value.Score descending
                select RoomInstance.Value.Tags).Take(50);
            var TagValues = new Dictionary<string, int>();
            foreach (var TagList in Tags)
            {
                foreach (var Tag in TagList)
                {
                    if (!TagValues.ContainsKey(Tag))
                    {
                        TagValues.Add(Tag, 1);
                    }
                    else
                    {
                        TagValues[Tag]++;
                    }
                }
            }

            var SortedTags = new List<KeyValuePair<string, int>>(TagValues);
            SortedTags.Sort((firstPair, nextPair) => firstPair.Value.CompareTo(nextPair.Value));
            SortedTags.Reverse();
            return SortedTags;
        }

        internal Room TryGetRandomLoadedRoom()
        {
            var room = (from RoomInstance in _rooms
                where RoomInstance.Value.RoomData.UsersNow > 0 &&
                      RoomInstance.Value.RoomData.Access == RoomAccess.OPEN &&
                      RoomInstance.Value.RoomData.UsersNow < RoomInstance.Value.RoomData.UsersMax
                orderby RoomInstance.Value.RoomData.UsersNow descending
                select RoomInstance.Value).Take(1);
            if (room.Any())
            {
                return room.First();
            }

            return null;
        }

        public RoomModel GetModel(string Model)
        {
            if (_roomModels.ContainsKey(Model))
            {
                return _roomModels[Model];
            }

            return null;
        }

        public RoomData GenerateRoomData(int RoomId)
        {
            if (_loadedRoomData.ContainsKey(RoomId))
            {
                return _loadedRoomData[RoomId];
            }

            var Data = new RoomData();
            Room Room;
            if (TryGetRoom(RoomId, out Room))
            {
                return Room.RoomData;
            }

            DataRow Row = null;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM rooms WHERE id = " + RoomId + " LIMIT 1");
                Row = dbClient.GetRow();
            }
            if (Row == null)
            {
                return null;
            }

            Data.Fill(Row);
            if (!_loadedRoomData.ContainsKey(RoomId))
            {
                _loadedRoomData.TryAdd(RoomId, Data);
            }
            return Data;
        }

        public RoomData FetchRoomData(int RoomId, DataRow dRow)
        {
            if (_loadedRoomData.ContainsKey(RoomId))
            {
                return _loadedRoomData[RoomId];
            }

            var data = new RoomData();
            data.Fill(dRow);
            if (!_loadedRoomData.ContainsKey(RoomId))
            {
                _loadedRoomData.TryAdd(RoomId, data);
            }
            return data;
        }

        public Room LoadRoom(int Id)
        {
            Room Room = null;
            if (TryGetRoom(Id, out Room))
            {
                return Room;
            }

            var Data = GenerateRoomData(Id);
            if (Data == null)
            {
                return null;
            }

            Room = new Room(Data);
            if (!_rooms.ContainsKey(Room.RoomId))
            {
                _rooms.TryAdd(Room.RoomId, Room);
            }
            return Room;
        }

        public bool TryGetRoom(int RoomId, out Room Room) => _rooms.TryGetValue(RoomId, out Room);

        public RoomData CreateRoom(GameClient Session,
                                   string Name,
                                   string Description,
                                   string Model,
                                   int Category,
                                   int MaxVisitors,
                                   int TradeSettings,
                                   string wallpaper = "0.0",
                                   string floor = "0.0",
                                   string landscape = "0.0",
                                   int wallthick = 0,
                                   int floorthick = 0)
        {
            if (!_roomModels.ContainsKey(Model))
            {
                Session.SendNotification(PlusEnvironment.GetLanguageManager().TryGetValue("room.creation.model.not_found"));
                return null;
            }

            if (Name.Length < 3)
            {
                Session.SendNotification(PlusEnvironment.GetLanguageManager().TryGetValue("room.creation.name.too_short"));
                return null;
            }

            var RoomId = 0;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "INSERT INTO `rooms` (`roomtype`,`caption`,`description`,`owner`,`model_name`,`category`,`users_max`,`trade_settings`,`wallpaper`,`floor`,`landscape`,`floorthick`,`wallthick`) VALUES ('private',@caption,@description,@UserId,@model,@category,@usersmax,@tradesettings,@wallpaper,@floor,@landscape,@floorthick,@wallthick)");
                dbClient.AddParameter("caption", Name);
                dbClient.AddParameter("description", Description);
                dbClient.AddParameter("UserId", Session.GetHabbo().Id);
                dbClient.AddParameter("model", Model);
                dbClient.AddParameter("category", Category);
                dbClient.AddParameter("usersmax", MaxVisitors);
                dbClient.AddParameter("tradesettings", TradeSettings);
                dbClient.AddParameter("wallpaper", wallpaper);
                dbClient.AddParameter("floor", floor);
                dbClient.AddParameter("landscape", landscape);
                dbClient.AddParameter("floorthick", floorthick);
                dbClient.AddParameter("wallthick", wallthick);
                RoomId = Convert.ToInt32(dbClient.InsertQuery());
            }
            var newRoomData = GenerateRoomData(RoomId);
            Session.GetHabbo().UsersRooms.Add(newRoomData);
            return newRoomData;
        }

        public ICollection<Room> GetRooms() => _rooms.Values;

        public void Dispose()
        {
            var length = _rooms.Count;
            var i = 0;
            foreach (var Room in _rooms.Values.ToList())
            {
                if (Room == null)
                {
                    continue;
                }

                PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(Room);
                Console.Clear();
                log.Info("<<- SERVER SHUTDOWN ->> ROOM ITEM SAVE: " + $"{(double) i / length * 100:0.##}" + "%");
                i++;
            }

            log.Info("Done disposing rooms!");
        }
    }
}