namespace Plus.HabboHotel.Rooms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using AI;
    using AI.Speech;
    using Communication.Interfaces;
    using Communication.Packets.Outgoing;
    using Communication.Packets.Outgoing.Rooms.Avatar;
    using Communication.Packets.Outgoing.Rooms.Engine;
    using Communication.Packets.Outgoing.Rooms.Session;
    using Core;
    using GameClients;
    using Games;
    using Games.Banzai;
    using Games.Football;
    using Games.Freeze;
    using Games.Teams;
    using Instance;
    using Items;
    using Items.Data.Moodlight;
    using Items.Data.Toner;

    public class Room : RoomData
    {
        private readonly BansComponent _bansComponent;

        private readonly FilterComponent _filterComponent;
        private readonly TradingComponent _tradingComponent;
        private readonly WiredComponent _wiredComponent;

        private readonly Dictionary<int, List<RoomUser>> Tents;
        private BattleBanzai _banzai;
        private Freeze _freeze;
        private GameItemHandler _gameItemHandler;
        private GameManager _gameManager;

        private Gamemap _gamemap;

        private RoomItemHandling _roomItemHandling;

        private RoomUserManager _roomUserManager;
        private Soccer _soccer;

        public ArrayList ActiveTrades;
        public bool isCrashed;
        public DateTime lastRegeneration;
        public DateTime lastTimerReset;
        public bool mDisposed;
        public MoodlightData MoodlightData;

        public Dictionary<int, double> MutedUsers;

        public Task ProcessTask;
        public bool RoomMuted;
        public TeamManager teambanzai;
        public TeamManager teamfreeze;

        public TonerData TonerData;

        public List<int> UsersWithRights;

        public Room(RoomData Data)
        {
            IsLagging = 0;
            IdleTime = 0;
            RoomData = Data;
            RoomMuted = false;
            mDisposed = false;
            Id = Data.Id;
            Name = Data.Name;
            Description = Data.Description;
            OwnerName = Data.OwnerName;
            OwnerId = Data.OwnerId;
            Category = Data.Category;
            Type = Data.Type;
            Access = Data.Access;
            UsersNow = 0;
            UsersMax = Data.UsersMax;
            ModelName = Data.ModelName;
            Score = Data.Score;
            Tags = new List<string>();
            foreach (var tag in Data.Tags)
            {
                Tags.Add(tag);
            }

            AllowPets = Data.AllowPets;
            AllowPetsEating = Data.AllowPetsEating;
            RoomBlockingEnabled = Data.RoomBlockingEnabled;
            Hidewall = Data.Hidewall;
            Group = Data.Group;
            Password = Data.Password;
            Wallpaper = Data.Wallpaper;
            Floor = Data.Floor;
            Landscape = Data.Landscape;
            WallThickness = Data.WallThickness;
            FloorThickness = Data.FloorThickness;
            chatMode = Data.chatMode;
            chatSize = Data.chatSize;
            chatSpeed = Data.chatSpeed;
            chatDistance = Data.chatDistance;
            extraFlood = Data.extraFlood;
            TradeSettings = Data.TradeSettings;
            WhoCanBan = Data.WhoCanBan;
            WhoCanKick = Data.WhoCanKick;
            WhoCanBan = Data.WhoCanBan;
            PushEnabled = Data.PushEnabled;
            PullEnabled = Data.PullEnabled;
            SPullEnabled = Data.SPullEnabled;
            SPushEnabled = Data.SPushEnabled;
            EnablesEnabled = Data.EnablesEnabled;
            RespectNotificationsEnabled = Data.RespectNotificationsEnabled;
            PetMorphsAllowed = Data.PetMorphsAllowed;
            ActiveTrades = new ArrayList();
            MutedUsers = new Dictionary<int, double>();
            Tents = new Dictionary<int, List<RoomUser>>();
            _gamemap = new Gamemap(this);
            if (_roomItemHandling == null)
            {
                _roomItemHandling = new RoomItemHandling(this);
            }
            _roomUserManager = new RoomUserManager(this);
            _filterComponent = new FilterComponent(this);
            _wiredComponent = new WiredComponent(this);
            _bansComponent = new BansComponent(this);
            _tradingComponent = new TradingComponent(this);
            GetRoomItemHandler().LoadFurniture();
            GetGameMap().GenerateMaps();
            LoadPromotions();
            LoadRights();
            LoadFilter();
            InitBots();
            InitPets();
            Data.UsersNow = 1;
        }

        public int IsLagging { get; set; }
        public int IdleTime { get; set; }

        public List<string> WordFilterList { get; set; }

        public int UserCount => _roomUserManager.GetRoomUsers().Count;

        public int RoomId => Id;

        public bool CanTradeInRoom => true;

        public RoomData RoomData { get; }

        public Gamemap GetGameMap() => _gamemap;

        public RoomItemHandling GetRoomItemHandler()
        {
            if (_roomItemHandling == null)
            {
                _roomItemHandling = new RoomItemHandling(this);
            }
            return _roomItemHandling;
        }

        public RoomUserManager GetRoomUserManager() => _roomUserManager;

        public Soccer GetSoccer()
        {
            if (_soccer == null)
            {
                _soccer = new Soccer(this);
            }
            return _soccer;
        }

        public TeamManager GetTeamManagerForBanzai()
        {
            if (teambanzai == null)
            {
                teambanzai = TeamManager.createTeamforGame("banzai");
            }
            return teambanzai;
        }

        public TeamManager GetTeamManagerForFreeze()
        {
            if (teamfreeze == null)
            {
                teamfreeze = TeamManager.createTeamforGame("freeze");
            }
            return teamfreeze;
        }

        public BattleBanzai GetBanzai()
        {
            if (_banzai == null)
            {
                _banzai = new BattleBanzai(this);
            }
            return _banzai;
        }

        public Freeze GetFreeze()
        {
            if (_freeze == null)
            {
                _freeze = new Freeze(this);
            }
            return _freeze;
        }

        public GameManager GetGameManager()
        {
            if (_gameManager == null)
            {
                _gameManager = new GameManager(this);
            }
            return _gameManager;
        }

        public GameItemHandler GetGameItemHandler()
        {
            if (_gameItemHandler == null)
            {
                _gameItemHandler = new GameItemHandler(this);
            }
            return _gameItemHandler;
        }

        public bool GotSoccer() => _soccer != null;

        public bool GotBanzai() => _banzai != null;

        public bool GotFreeze() => _freeze != null;

        public void ClearTags()
        {
            Tags.Clear();
        }

        public void AddTagRange(List<string> tags)
        {
            Tags.AddRange(tags);
        }

        public void InitBots()
        {
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `id`,`room_id`,`name`,`motto`,`look`,`x`,`y`,`z`,`rotation`,`gender`,`user_id`,`ai_type`,`walk_mode`,`automatic_chat`,`speaking_interval`,`mix_sentences`,`chat_bubble` FROM `bots` WHERE `room_id` = '" +
                    RoomId +
                    "' AND `ai_type` != 'pet'");
                var Data = dbClient.GetTable();
                if (Data == null)
                {
                    return;
                }

                foreach (DataRow Bot in Data.Rows)
                {
                    dbClient.SetQuery("SELECT `text` FROM `bots_speech` WHERE `bot_id` = '" + Convert.ToInt32(Bot["id"]) + "'");
                    var BotSpeech = dbClient.GetTable();
                    var Speeches = new List<RandomSpeech>();
                    foreach (DataRow Speech in BotSpeech.Rows)
                    {
                        Speeches.Add(new RandomSpeech(Convert.ToString(Speech["text"]), Convert.ToInt32(Bot["id"])));
                    }

                    _roomUserManager.DeployBot(new RoomBot(Convert.ToInt32(Bot["id"]),
                            Convert.ToInt32(Bot["room_id"]),
                            Convert.ToString(Bot["ai_type"]),
                            Convert.ToString(Bot["walk_mode"]),
                            Convert.ToString(Bot["name"]),
                            Convert.ToString(Bot["motto"]),
                            Convert.ToString(Bot["look"]),
                            int.Parse(Bot["x"].ToString()),
                            int.Parse(Bot["y"].ToString()),
                            int.Parse(Bot["z"].ToString()),
                            int.Parse(Bot["rotation"].ToString()),
                            0,
                            0,
                            0,
                            0,
                            ref Speeches,
                            "M",
                            0,
                            Convert.ToInt32(Bot["user_id"].ToString()),
                            Convert.ToBoolean(Bot["automatic_chat"]),
                            Convert.ToInt32(Bot["speaking_interval"]),
                            PlusEnvironment.EnumToBool(Bot["mix_sentences"].ToString()),
                            Convert.ToInt32(Bot["chat_bubble"])),
                        null);
                }
            }
        }

        public void InitPets()
        {
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`user_id`,`room_id`,`name`,`x`,`y`,`z` FROM `bots` WHERE `room_id` = '" +
                                  RoomId +
                                  "' AND `ai_type` = 'pet'");
                var Data = dbClient.GetTable();
                if (Data == null)
                {
                    return;
                }

                foreach (DataRow Row in Data.Rows)
                {
                    dbClient.SetQuery(
                        "SELECT `type`,`race`,`color`,`experience`,`energy`,`nutrition`,`respect`,`createstamp`,`have_saddle`,`anyone_ride`,`hairdye`,`pethair`,`gnome_clothing` FROM `bots_petdata` WHERE `id` = '" +
                        Row[0] +
                        "' LIMIT 1");
                    var mRow = dbClient.GetRow();
                    if (mRow == null)
                    {
                        continue;
                    }

                    var Pet = new Pet(Convert.ToInt32(Row["id"]),
                        Convert.ToInt32(Row["user_id"]),
                        Convert.ToInt32(Row["room_id"]),
                        Convert.ToString(Row["name"]),
                        Convert.ToInt32(mRow["type"]),
                        Convert.ToString(mRow["race"]),
                        Convert.ToString(mRow["color"]),
                        Convert.ToInt32(mRow["experience"]),
                        Convert.ToInt32(mRow["energy"]),
                        Convert.ToInt32(mRow["nutrition"]),
                        Convert.ToInt32(mRow["respect"]),
                        Convert.ToDouble(mRow["createstamp"]),
                        Convert.ToInt32(Row["x"]),
                        Convert.ToInt32(Row["y"]),
                        Convert.ToDouble(Row["z"]),
                        Convert.ToInt32(mRow["have_saddle"]),
                        Convert.ToInt32(mRow["anyone_ride"]),
                        Convert.ToInt32(mRow["hairdye"]),
                        Convert.ToInt32(mRow["pethair"]),
                        Convert.ToString(mRow["gnome_clothing"]));
                    var RndSpeechList = new List<RandomSpeech>();
                    _roomUserManager.DeployBot(new RoomBot(Pet.PetId,
                            RoomId,
                            "pet",
                            "freeroam",
                            Pet.Name,
                            "",
                            Pet.Look,
                            Pet.X,
                            Pet.Y,
                            Convert.ToInt32(Pet.Z),
                            0,
                            0,
                            0,
                            0,
                            0,
                            ref RndSpeechList,
                            "",
                            0,
                            Pet.OwnerId,
                            false,
                            0,
                            false,
                            0),
                        Pet);
                }
            }
        }

        public FilterComponent GetFilter() => _filterComponent;

        public WiredComponent GetWired() => _wiredComponent;

        public BansComponent GetBans() => _bansComponent;

        public TradingComponent GetTrading() => _tradingComponent;

        public void LoadPromotions()
        {
            DataRow GetPromotion = null;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `room_promotions` WHERE `room_id` = " + Id + " LIMIT 1;");
                GetPromotion = dbClient.GetRow();
                if (GetPromotion != null)
                {
                    if (Convert.ToDouble(GetPromotion["timestamp_expire"]) > PlusEnvironment.GetUnixTimestamp())
                    {
                        RoomData._promotion = new RoomPromotion(Convert.ToString(GetPromotion["title"]),
                            Convert.ToString(GetPromotion["description"]),
                            Convert.ToDouble(GetPromotion["timestamp_start"]),
                            Convert.ToDouble(GetPromotion["timestamp_expire"]),
                            Convert.ToInt32(GetPromotion["category_id"]));
                    }
                }
            }
        }

        public void LoadRights()
        {
            UsersWithRights = new List<int>();
            if (Group != null)
            {
                return;
            }

            DataTable Data = null;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT room_rights.user_id FROM room_rights WHERE room_id = @roomid");
                dbClient.AddParameter("roomid", Id);
                Data = dbClient.GetTable();
            }
            if (Data != null)
            {
                foreach (DataRow Row in Data.Rows)
                {
                    UsersWithRights.Add(Convert.ToInt32(Row["user_id"]));
                }
            }
        }

        private void LoadFilter()
        {
            WordFilterList = new List<string>();
            DataTable Data = null;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `room_filter` WHERE `room_id` = @roomid;");
                dbClient.AddParameter("roomid", Id);
                Data = dbClient.GetTable();
            }
            if (Data == null)
            {
                return;
            }

            foreach (DataRow Row in Data.Rows)
            {
                WordFilterList.Add(Convert.ToString(Row["word"]));
            }
        }

        public bool CheckRights(GameClient Session) => CheckRights(Session, false);

        public bool CheckRights(GameClient Session, bool RequireOwnership, bool CheckForGroups = false)
        {
            try
            {
                if (Session == null || Session.GetHabbo() == null)
                {
                    return false;
                }
                if (Session.GetHabbo().Username == OwnerName && Type == "private")
                {
                    return true;
                }
                if (Session.GetHabbo().GetPermissions().HasRight("room_any_owner"))
                {
                    return true;
                }

                if (!RequireOwnership && Type == "private")
                {
                    if (Session.GetHabbo().GetPermissions().HasRight("room_any_rights"))
                    {
                        return true;
                    }
                    if (UsersWithRights.Contains(Session.GetHabbo().Id))
                    {
                        return true;
                    }
                }

                if (CheckForGroups && Type == "private")
                {
                    if (Group == null)
                    {
                        return false;
                    }
                    if (Group.IsAdmin(Session.GetHabbo().Id))
                    {
                        return true;
                    }

                    if (Group.AdminOnlyDeco == 0)
                    {
                        if (Group.IsAdmin(Session.GetHabbo().Id))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }

            return false;
        }

        public void OnUserShoot(RoomUser User, Item Ball)
        {
            Func<Item, bool> predicate = null;
            string Key = null;
            foreach (var item in GetRoomItemHandler().GetFurniObjects(Ball.GetX, Ball.GetY).ToList())
            {
                if (item.GetBaseItem().ItemName.StartsWith("fball_goal_"))
                {
                    Key = item.GetBaseItem().ItemName.Split('_')[2];
                    User.UnIdle();
                    User.DanceId = 0;
                    PlusEnvironment.GetGame().GetAchievementManager()
                        .ProgressAchievement(User.GetClient(), "ACH_FootballGoalScored", 1);
                    SendPacket(new ActionComposer(User.VirtualId, 1));
                }
            }

            if (Key != null)
            {
                if (predicate == null)
                {
                    predicate = p => p.GetBaseItem().ItemName == "fball_score_" + Key;
                }
                foreach (var item2 in GetRoomItemHandler().GetFloor.Where(predicate).ToList())
                {
                    if (item2.GetBaseItem().ItemName == "fball_score_" + Key)
                    {
                        if (!string.IsNullOrEmpty(item2.ExtraData))
                        {
                            item2.ExtraData = (Convert.ToInt32(item2.ExtraData) + 1).ToString();
                        }
                        else
                        {
                            item2.ExtraData = "1";
                        }
                        item2.UpdateState();
                    }
                }
            }
        }

        public void ProcessRoom()
        {
            if (isCrashed || mDisposed)
            {
                return;
            }

            try
            {
                if (GetRoomUserManager().GetRoomUsers().Count == 0)
                {
                    IdleTime++;
                }
                else if (IdleTime > 0)
                {
                    IdleTime = 0;
                }
                if (RoomData.HasActivePromotion && RoomData.Promotion.HasExpired)
                {
                    RoomData.EndPromotion();
                }
                if (IdleTime >= 60 && !RoomData.HasActivePromotion)
                {
                    PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(this);
                    return;
                }

                try
                {
                    GetRoomItemHandler().OnCycle();
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);
                }
                try
                {
                    GetRoomUserManager().OnCycle();
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);
                }
                try
                {
                    GetRoomUserManager().SerializeStatusUpdates();
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);
                }
                try
                {
                    if (_gameItemHandler != null)
                    {
                        _gameItemHandler.OnCycle();
                    }
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);
                }
                try
                {
                    GetWired().OnCycle();
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
                OnRoomCrash(e);
            }
        }

        private void OnRoomCrash(Exception e)
        {
            try
            {
                foreach (var user in _roomUserManager.GetRoomUsers().ToList())
                {
                    if (user == null || user.GetClient() == null)
                    {
                        continue;
                    }

                    user.GetClient()
                        .SendNotification("Sorry, it appears that room has crashed!"); //Unhandled exception in room: " + e);
                    try
                    {
                        GetRoomUserManager().RemoveUserFromRoom(user.GetClient(), true);
                    }
                    catch (Exception e2)
                    {
                        ExceptionLogger.LogException(e2);
                    }
                }
            }
            catch (Exception e3)
            {
                ExceptionLogger.LogException(e3);
            }

            isCrashed = true;
            PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(this, true);
        }

        public bool CheckMute(GameClient Session)
        {
            if (MutedUsers.ContainsKey(Session.GetHabbo().Id))
            {
                if (MutedUsers[Session.GetHabbo().Id] < PlusEnvironment.GetUnixTimestamp())
                {
                    MutedUsers.Remove(Session.GetHabbo().Id);
                }
                else
                {
                    return true;
                }
            }

            if (Session.GetHabbo().TimeMuted > 0 || RoomMuted && Session.GetHabbo().Username != OwnerName)
            {
                return true;
            }

            return false;
        }

        public void SendObjects(GameClient Session)
        {
            var Room = Session.GetHabbo().CurrentRoom;
            Session.SendPacket(new HeightMapComposer(Room.GetGameMap().Model.Heightmap));
            Session.SendPacket(new FloorHeightMapComposer(Room.GetGameMap().Model.GetRelativeHeightmap(),
                Room.GetGameMap().StaticModel.WallHeight));
            foreach (var RoomUser in _roomUserManager.GetUserList().ToList())
            {
                if (RoomUser == null)
                {
                    continue;
                }

                Session.SendPacket(new UsersComposer(RoomUser));
                if (RoomUser.IsBot && RoomUser.BotData.DanceId > 0)
                {
                    Session.SendPacket(new DanceComposer(RoomUser, RoomUser.BotData.DanceId));
                }
                else if (!RoomUser.IsBot && !RoomUser.IsPet && RoomUser.IsDancing)
                {
                    Session.SendPacket(new DanceComposer(RoomUser, RoomUser.DanceId));
                }
                if (RoomUser.IsAsleep)
                {
                    Session.SendPacket(new SleepComposer(RoomUser, true));
                }
                if (RoomUser.CarryItemID > 0 && RoomUser.CarryTimer > 0)
                {
                    Session.SendPacket(new CarryObjectComposer(RoomUser.VirtualId, RoomUser.CarryItemID));
                }
                if (!RoomUser.IsBot && !RoomUser.IsPet && RoomUser.CurrentEffect > 0)
                {
                    Session.SendPacket(new AvatarEffectComposer(RoomUser.VirtualId, RoomUser.CurrentEffect));
                }
            }

            Session.SendPacket(new UserUpdateComposer(_roomUserManager.GetUserList().ToList()));
            Session.SendPacket(new ObjectsComposer(Room.GetRoomItemHandler().GetFloor.ToArray(), this));
            Session.SendPacket(new ItemsComposer(Room.GetRoomItemHandler().GetWall.ToArray(), this));
        }

        public void AddTent(int TentId)
        {
            if (Tents.ContainsKey(TentId))
            {
                Tents.Remove(TentId);
            }
            Tents.Add(TentId, new List<RoomUser>());
        }

        public void RemoveTent(int TentId)
        {
            if (!Tents.ContainsKey(TentId))
            {
                return;
            }

            var Users = Tents[TentId];
            foreach (var User in Users.ToList())
            {
                if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null)
                {
                    continue;
                }

                User.GetClient().GetHabbo().TentId = 0;
            }

            if (Tents.ContainsKey(TentId))
            {
                Tents.Remove(TentId);
            }
        }

        public void AddUserToTent(int TentId, RoomUser User)
        {
            if (User != null && User.GetClient() != null && User.GetClient().GetHabbo() != null)
            {
                if (!Tents.ContainsKey(TentId))
                {
                    Tents.Add(TentId, new List<RoomUser>());
                }
                if (!Tents[TentId].Contains(User))
                {
                    Tents[TentId].Add(User);
                }
                User.GetClient().GetHabbo().TentId = TentId;
            }
        }

        public void RemoveUserFromTent(int TentId, RoomUser User)
        {
            if (User != null && User.GetClient() != null && User.GetClient().GetHabbo() != null)
            {
                if (!Tents.ContainsKey(TentId))
                {
                    Tents.Add(TentId, new List<RoomUser>());
                }
                if (Tents[TentId].Contains(User))
                {
                    Tents[TentId].Remove(User);
                }
                User.GetClient().GetHabbo().TentId = 0;
            }
        }

        public void SendToTent(int Id, int TentId, IServerPacket Packet)
        {
            if (!Tents.ContainsKey(TentId))
            {
                return;
            }

            foreach (var User in Tents[TentId].ToList())
            {
                if (User == null ||
                    User.GetClient() == null ||
                    User.GetClient().GetHabbo() == null ||
                    User.GetClient().GetHabbo().GetIgnores().IgnoredUserIds().Contains(Id) ||
                    User.GetClient().GetHabbo().TentId != TentId)
                {
                    continue;
                }

                User.GetClient().SendPacket(Packet);
            }
        }

        public void SendPacket(IServerPacket Message, bool UsersWithRightsOnly = false)
        {
            if (Message == null)
            {
                return;
            }

            try
            {
                var Users = _roomUserManager.GetUserList().ToList();
                if (this == null || _roomUserManager == null || Users == null)
                {
                    return;
                }

                foreach (var User in Users)
                {
                    if (User == null || User.IsBot)
                    {
                        continue;
                    }
                    if (User.GetClient() == null || User.GetClient().GetConnection() == null)
                    {
                        continue;
                    }
                    if (UsersWithRightsOnly && !CheckRights(User.GetClient()))
                    {
                        continue;
                    }

                    User.GetClient().SendPacket(Message);
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        public void BroadcastPacket(byte[] Packet)
        {
            foreach (var User in _roomUserManager.GetUserList().ToList())
            {
                if (User == null || User.IsBot)
                {
                    continue;
                }
                if (User.GetClient() == null || User.GetClient().GetConnection() == null)
                {
                    continue;
                }

                User.GetClient().GetConnection().SendData(Packet);
            }
        }

        public void SendPacket(List<ServerPacket> Messages)
        {
            if (Messages.Count == 0)
            {
                return;
            }

            try
            {
                var TotalBytes = new byte[0];
                var Current = 0;
                foreach (var Packet in Messages.ToList())
                {
                    var ToAdd = Packet.GetBytes();
                    var NewLen = TotalBytes.Length + ToAdd.Length;
                    Array.Resize(ref TotalBytes, NewLen);
                    for (var i = 0; i < ToAdd.Length; i++)
                    {
                        TotalBytes[Current] = ToAdd[i];
                        Current++;
                    }
                }

                BroadcastPacket(TotalBytes);
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        public void Dispose()
        {
            SendPacket(new CloseConnectionComposer());
            if (!mDisposed)
            {
                isCrashed = false;
                mDisposed = true;
                /* TODO: Needs reviewing */
                try
                {
                    if (ProcessTask != null && ProcessTask.IsCompleted)
                    {
                        ProcessTask.Dispose();
                    }
                }
                catch
                {
                }
                if (ActiveTrades.Count > 0)
                {
                    ActiveTrades.Clear();
                }
                TonerData = null;
                MoodlightData = null;
                if (MutedUsers.Count > 0)
                {
                    MutedUsers.Clear();
                }
                if (Tents.Count > 0)
                {
                    Tents.Clear();
                }
                if (UsersWithRights.Count > 0)
                {
                    UsersWithRights.Clear();
                }
                if (_gameManager != null)
                {
                    _gameManager.Dispose();
                    _gameManager = null;
                }
                if (_freeze != null)
                {
                    _freeze.Dispose();
                    _freeze = null;
                }
                if (_soccer != null)
                {
                    _soccer.Dispose();
                    _soccer = null;
                }
                if (_banzai != null)
                {
                    _banzai.Dispose();
                    _banzai = null;
                }
                if (_gamemap != null)
                {
                    _gamemap.Dispose();
                    _gamemap = null;
                }
                if (_gameItemHandler != null)
                {
                    _gameItemHandler.Dispose();
                    _gameItemHandler = null;
                }

                // Room Data?
                if (teambanzai != null)
                {
                    teambanzai.Dispose();
                    teambanzai = null;
                }
                if (teamfreeze != null)
                {
                    teamfreeze.Dispose();
                    teamfreeze = null;
                }
                if (_roomUserManager != null)
                {
                    _roomUserManager.Dispose();
                    _roomUserManager = null;
                }
                if (_roomItemHandling != null)
                {
                    _roomItemHandling.Dispose();
                    _roomItemHandling = null;
                }
                if (WordFilterList.Count > 0)
                {
                    WordFilterList.Clear();
                }
                if (_filterComponent != null)
                {
                    _filterComponent.Cleanup();
                }
                if (_wiredComponent != null)
                {
                    _wiredComponent.Cleanup();
                }
                if (_bansComponent != null)
                {
                    _bansComponent.Cleanup();
                }
                if (_tradingComponent != null)
                {
                    _tradingComponent.Cleanup();
                }
            }
        }
    }
}