namespace Plus.HabboHotel.Rooms.AI
{
    using System;
    using Communication.Packets.Outgoing.Pets;
    using Communication.Packets.Outgoing.Rooms.AI.Pets;
    using Communication.Packets.Outgoing.Rooms.Chat;

    public class Pet
    {
        private readonly int[] _experienceLevels =
        {
            100,
            200,
            400,
            600,
            1000,
            1300,
            1800,
            2400,
            3200,
            4300,
            7200,
            8500,
            10100,
            13300,
            17500,
            23000,
            51900,
            75000,
            128000,
            150000
        };

        internal int AnyoneCanRide;
        internal string Color;
        internal double CreationStamp;
        internal DatabaseUpdateState DbState;

        internal int Energy;
        internal int Experience;

        internal string GnomeClothing;
        internal int HairDye;
        internal string Name;
        internal int Nutrition;
        internal int OwnerId;
        internal int PetHair;
        internal int PetId;
        internal bool PlacedInRoom;
        internal string Race;
        internal int Respect;
        internal int RoomId;
        internal int Saddle;
        internal int Type;
        internal int VirtualId;
        internal int X;
        internal int Y;
        internal double Z;

        internal Pet(int petId,
                     int ownerId,
                     int roomId,
                     string name,
                     int type,
                     string race,
                     string color,
                     int experience,
                     int energy,
                     int nutrition,
                     int respect,
                     double creationStamp,
                     int x,
                     int y,
                     double z,
                     int saddle,
                     int anyonecanride,
                     int dye,
                     int petHer,
                     string gnomeClothing)
        {
            PetId = petId;
            OwnerId = ownerId;
            RoomId = roomId;
            Name = name;
            Type = type;
            Race = race;
            Color = color;
            Experience = experience;
            Energy = energy;
            Nutrition = nutrition;
            Respect = respect;
            CreationStamp = creationStamp;
            X = x;
            Y = y;
            Z = z;
            PlacedInRoom = false;
            DbState = DatabaseUpdateState.Updated;
            Saddle = saddle;
            AnyoneCanRide = anyonecanride;
            PetHair = petHer;
            HairDye = dye;
            GnomeClothing = gnomeClothing;
        }

        public Room Room
        {
            get
            {
                if (!IsInRoom)
                {
                    return null;
                }

                Room room;
                if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(RoomId, out room))
                {
                    return room;
                }

                return null;
            }
        }

        public bool IsInRoom => RoomId > 0;

        public int Level
        {
            get
            {
                for (var level = 0; level < _experienceLevels.Length; ++level)
                {
                    if (Experience < _experienceLevels[level])
                    {
                        return level + 1;
                    }
                }

                return _experienceLevels.Length;
            }
        }

        public static int MaxLevel => 20;

        public int ExperienceGoal => _experienceLevels[Level - 1];

        public static int MaxEnergy => 100;

        public static int MaxNutrition => 150;

        public int Age => Convert.ToInt32(Math.Floor((PlusEnvironment.GetUnixTimestamp() - CreationStamp) / 86400));

        public string Look => Type + " " + Race + " " + Color + " " + GnomeClothing;

        public string OwnerName => PlusEnvironment.GetGame().GetClientManager().GetNameById(OwnerId);

        public void OnRespect()
        {
            Respect++;
            Room.SendPacket(new RespectPetNotificationMessageComposer(this));
            if (DbState != DatabaseUpdateState.NeedsInsert)
            {
                DbState = DatabaseUpdateState.NeedsUpdate;
            }
            if (Experience <= 150000)
            {
                Addexperience(10);
            }
        }

        public void Addexperience(int amount)
        {
            Experience = Experience + amount;
            if (Experience > 150000)
            {
                Experience = 150000;
                if (Room != null)
                {
                    Room.SendPacket(new AddExperiencePointsComposer(PetId, VirtualId, amount));
                }
                return;
            }

            if (DbState != DatabaseUpdateState.NeedsInsert)
            {
                DbState = DatabaseUpdateState.NeedsUpdate;
            }
            if (Room != null)
            {
                Room.SendPacket(new AddExperiencePointsComposer(PetId, VirtualId, amount));
                if (Experience >= ExperienceGoal)
                {
                    Room.SendPacket(new ChatComposer(VirtualId, "*leveled up to level " + Level + " *", 0, 0));
                }
            }
        }

        public void PetEnergy(bool add)
        {
            int maxE;
            if (add)
            {
                if (Energy == 100) // If Energy is 100, no point.
                {
                    return;
                }

                if (Energy > 85)
                {
                    maxE = MaxEnergy - Energy;
                }
                else
                {
                    maxE = 10;
                }
            }
            else
            {
                maxE = 15; // Remove Max Energy as 15
            }

            if (maxE <= 4)
            {
                maxE = 15;
            }
            var r = PlusEnvironment.GetRandomNumber(4, maxE);
            if (!add)
            {
                Energy = Energy - r;
                if (Energy < 0)
                {
                    Energy = 1;
                    r = 1;
                }
            }
            else
            {
                Energy = Energy + r;
            }
            if (DbState != DatabaseUpdateState.NeedsInsert)
            {
                DbState = DatabaseUpdateState.NeedsUpdate;
            }
        }
    }

    public enum DatabaseUpdateState
    {
        Updated,
        NeedsUpdate,
        NeedsInsert
    }
}