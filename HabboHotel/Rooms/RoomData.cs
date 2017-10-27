namespace Plus.HabboHotel.Rooms
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Groups;

    public class RoomData
    {
        public RoomPromotion _promotion;
        public RoomAccess Access;
        public int AllowPets;
        public int AllowPetsEating;
        public int Category;
        public int chatDistance;
        public int chatMode;
        public int chatSize;
        public int chatSpeed;
        public string Description;
        public bool EnablesEnabled;
        public int extraFlood;
        public string Floor;
        public int FloorThickness;
        public Group Group;
        public int Hidewall;
        public int Id;
        public string Landscape;
        private RoomModel mModel;
        public string ModelName;
        public string Name;
        public int OwnerId;
        public string OwnerName;
        public string Password;
        public bool PetMorphsAllowed;
        public bool PullEnabled;

        public bool PushEnabled;
        public bool RespectNotificationsEnabled;
        public int RoomBlockingEnabled;
        public int Score;
        public bool SPullEnabled;
        public bool SPushEnabled;
        public List<string> Tags;

        public int TradeSettings; //Default = 2;
        public string Type;
        public int UsersMax;
        public int UsersNow;
        public string Wallpaper;
        public int WallThickness;
        public int WhoCanBan;
        public int WhoCanKick;
        public int WhoCanMute;

        public RoomPromotion Promotion
        {
            get => _promotion;
            set => _promotion = value;
        }

        public bool HasActivePromotion => Promotion != null;

        public RoomModel Model
        {
            get
            {
                if (mModel == null)
                {
                    mModel = PlusEnvironment.GetGame().GetRoomManager().GetModel(ModelName);
                }
                return mModel;
            }
        }

        public void Fill(DataRow Row)
        {
            Id = Convert.ToInt32(Row["id"]);
            Name = Convert.ToString(Row["caption"]);
            Description = Convert.ToString(Row["description"]);
            Type = Convert.ToString(Row["roomtype"]);
            OwnerId = Convert.ToInt32(Row["owner"]);
            OwnerName = "";
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `username` FROM `users` WHERE `id` = @owner LIMIT 1");
                dbClient.AddParameter("owner", OwnerId);
                var result = dbClient.GetString();
                if (!string.IsNullOrEmpty(result))
                {
                    OwnerName = result;
                }
            }
            Access = RoomAccessUtility.ToRoomAccess(Row["state"].ToString().ToLower());
            Category = Convert.ToInt32(Row["category"]);
            if (!string.IsNullOrEmpty(Row["users_now"].ToString()))
            {
                UsersNow = Convert.ToInt32(Row["users_now"]);
            }
            else
            {
                UsersNow = 0;
            }
            UsersMax = Convert.ToInt32(Row["users_max"]);
            ModelName = Convert.ToString(Row["model_name"]);
            Score = Convert.ToInt32(Row["score"]);
            Tags = new List<string>();
            AllowPets = Convert.ToInt32(Row["allow_pets"].ToString());
            AllowPetsEating = Convert.ToInt32(Row["allow_pets_eat"].ToString());
            RoomBlockingEnabled = Convert.ToInt32(Row["room_blocking_disabled"].ToString());
            Hidewall = Convert.ToInt32(Row["allow_hidewall"].ToString());
            Password = Convert.ToString(Row["password"]);
            Wallpaper = Convert.ToString(Row["wallpaper"]);
            Floor = Convert.ToString(Row["floor"]);
            Landscape = Convert.ToString(Row["landscape"]);
            FloorThickness = Convert.ToInt32(Row["floorthick"]);
            WallThickness = Convert.ToInt32(Row["wallthick"]);
            WhoCanMute = Convert.ToInt32(Row["mute_settings"]);
            WhoCanKick = Convert.ToInt32(Row["kick_settings"]);
            WhoCanBan = Convert.ToInt32(Row["ban_settings"]);
            chatMode = Convert.ToInt32(Row["chat_mode"]);
            chatSpeed = Convert.ToInt32(Row["chat_speed"]);
            chatSize = Convert.ToInt32(Row["chat_size"]);
            TradeSettings = Convert.ToInt32(Row["trade_settings"]);
            Group G = null;
            if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(Convert.ToInt32(Row["group_id"]), out G))
            {
                Group = G;
            }
            else
            {
                Group = null;
            }
            foreach (var Tag in Row["tags"].ToString().Split(','))
            {
                Tags.Add(Tag);
            }

            mModel = PlusEnvironment.GetGame().GetRoomManager().GetModel(ModelName);
            PushEnabled = PlusEnvironment.EnumToBool(Row["push_enabled"].ToString());
            PullEnabled = PlusEnvironment.EnumToBool(Row["pull_enabled"].ToString());
            SPushEnabled = PlusEnvironment.EnumToBool(Row["spush_enabled"].ToString());
            SPullEnabled = PlusEnvironment.EnumToBool(Row["spull_enabled"].ToString());
            EnablesEnabled = PlusEnvironment.EnumToBool(Row["enables_enabled"].ToString());
            RespectNotificationsEnabled = PlusEnvironment.EnumToBool(Row["respect_notifications_enabled"].ToString());
            PetMorphsAllowed = PlusEnvironment.EnumToBool(Row["pet_morphs_allowed"].ToString());
        }

        public void EndPromotion()
        {
            if (!HasActivePromotion)
            {
                return;
            }

            Promotion = null;
        }
    }
}