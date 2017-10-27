namespace Plus.HabboHotel.Rooms
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Core;
    using Games.Teams;
    using Groups;
    using Items;
    using PathFinding;

    public class Gamemap
    {
        private Room _room;
        public bool DiagonalEnabled;

        public bool gotPublicPool;
        private ConcurrentDictionary<Point, List<int>> mCoordinatedItems;
        private double[,] mItemHeightMap;
        private ConcurrentDictionary<Point, List<RoomUser>> userMap;

        public Gamemap(Room room)
        {
            _room = room;
            DiagonalEnabled = true;
            StaticModel = PlusEnvironment.GetGame().GetRoomManager().GetModel(room.ModelName);
            if (StaticModel == null)
            {
                PlusEnvironment.GetGame().GetRoomManager().LoadModel(room.ModelName);
                StaticModel = PlusEnvironment.GetGame().GetRoomManager().GetModel(room.ModelName);
            }
            if (StaticModel == null)
            {
                return;
            }

            Model = new DynamicRoomModel(StaticModel);
            mCoordinatedItems = new ConcurrentDictionary<Point, List<int>>();
            gotPublicPool = room.RoomData.Model.gotPublicPool;
            GameMap = new byte[Model.MapSizeX, Model.MapSizeY];
            mItemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];
            userMap = new ConcurrentDictionary<Point, List<RoomUser>>();
        }

        public DynamicRoomModel Model { get; private set; }

        public RoomModel StaticModel { get; private set; }

        public byte[,] EffectMap { get; private set; }

        public byte[,] GameMap { get; private set; }

        public void AddUserToMap(RoomUser user, Point coord)
        {
            if (userMap.ContainsKey(coord))
            {
                userMap[coord].Add(user);
            }
            else
            {
                var users = new List<RoomUser>();
                users.Add(user);
                userMap.TryAdd(coord, users);
            }
        }

        public void TeleportToItem(RoomUser user, Item item)
        {
            if (item == null || user == null)
            {
                return;
            }

            GameMap[user.X, user.Y] = user.SqState;
            UpdateUserMovement(new Point(user.Coordinate.X, user.Coordinate.Y), new Point(item.Coordinate.X, item.Coordinate.Y),
                user);
            user.X = item.GetX;
            user.Y = item.GetY;
            user.Z = item.GetZ;
            user.SqState = GameMap[item.GetX, item.GetY];
            GameMap[user.X, user.Y] = 1;
            user.RotBody = item.Rotation;
            user.RotHead = item.Rotation;
            user.GoalX = user.X;
            user.GoalY = user.Y;
            user.SetStep = false;
            user.IsWalking = false;
            user.UpdateNeeded = true;
        }

        public void UpdateUserMovement(Point oldCoord, Point newCoord, RoomUser user)
        {
            RemoveUserFromMap(user, oldCoord);
            AddUserToMap(user, newCoord);
        }

        public void RemoveUserFromMap(RoomUser user, Point coord)
        {
            if (userMap.ContainsKey(coord))
            {
                userMap[coord].RemoveAll(x => x != null && x.VirtualId == user.VirtualId);
            }
        }

        public bool MapGotUser(Point coord) => GetRoomUsers(coord).Count > 0;

        public List<RoomUser> GetRoomUsers(Point coord)
        {
            if (userMap.ContainsKey(coord))
            {
                return userMap[coord];
            }

            return new List<RoomUser>();
        }

        internal Point GetRandomWalkableSquare()
        {
            var walkableSquares = new List<Point>();
            for (var y = 0; y < GameMap.GetUpperBound(1); y++)
            {
                for (var x = 0; x < GameMap.GetUpperBound(0); x++)
                {
                    if (StaticModel.DoorX != x && StaticModel.DoorY != y && GameMap[x, y] == 1)
                    {
                        walkableSquares.Add(new Point(x, y));
                    }
                }
            }

            var randomNumber = PlusEnvironment.GetRandomNumber(0, walkableSquares.Count);
            var i = 0;
            
            foreach (var coord in walkableSquares.ToList())
            {
                if (i == randomNumber)
                {
                    return coord;
                }

                i++;
            }

            return new Point(0, 0);
        }

        internal void AddToMap(Item item)
        {
            AddItemToMap(item);
        }

        private void SetDefaultValue(int x, int y)
        {
            GameMap[x, y] = 0;
            EffectMap[x, y] = 0;
            mItemHeightMap[x, y] = 0.0;
            if (x == Model.DoorX && y == Model.DoorY)
            {
                GameMap[x, y] = 3;
            }
            else if (Model.SqState[x, y] == SquareState.OPEN)
            {
                GameMap[x, y] = 1;
            }
            else if (Model.SqState[x, y] == SquareState.SEAT)
            {
                GameMap[x, y] = 2;
            }
        }

        internal void UpdateMapForItem(Item item)
        {
            RemoveFromMap(item);
            AddToMap(item);
        }

        internal void GenerateMaps(bool checkLines = true)
        {
            var maxX = 0;
            var maxY = 0;
            
            mCoordinatedItems = new ConcurrentDictionary<Point, List<int>>();
            if (checkLines)
            {
                var items = _room.GetRoomItemHandler().GetFloor.ToArray();
                foreach (var item in items.ToList())
                {
                    if (item == null)
                    {
                        continue;
                    }

                    if (item.GetX > Model.MapSizeX && item.GetX > maxX)
                    {
                        maxX = item.GetX;
                    }
                    if (item.GetY > Model.MapSizeY && item.GetY > maxY)
                    {
                        maxY = item.GetY;
                    }
                }

                Array.Clear(items, 0, items.Length);
            }

            if (maxY > Model.MapSizeY - 1 || maxX > Model.MapSizeX - 1)
            {
                if (maxX < Model.MapSizeX)
                {
                    maxX = Model.MapSizeX;
                }
                if (maxY < Model.MapSizeY)
                {
                    maxY = Model.MapSizeY;
                }
                Model.SetMapsize(maxX + 7, maxY + 7);
                GenerateMaps(false);
                return;
            }

            if (maxX != StaticModel.MapSizeX || maxY != StaticModel.MapSizeY)
            {
                EffectMap = new byte[Model.MapSizeX, Model.MapSizeY];
                GameMap = new byte[Model.MapSizeX, Model.MapSizeY];
                mItemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];

                //if (modelRemap)
                //    Model.Generate(); //Clears model
                for (var line = 0; line < Model.MapSizeY; line++)
                {
                    for (var chr = 0; chr < Model.MapSizeX; chr++)
                    {
                        GameMap[chr, line] = 0;
                        EffectMap[chr, line] = 0;
                        if (chr == Model.DoorX && line == Model.DoorY)
                        {
                            GameMap[chr, line] = 3;
                        }
                        else if (Model.SqState[chr, line] == SquareState.OPEN)
                        {
                            GameMap[chr, line] = 1;
                        }
                        else if (Model.SqState[chr, line] == SquareState.SEAT)
                        {
                            GameMap[chr, line] = 2;
                        }
                        else if (Model.SqState[chr, line] == SquareState.POOL)
                        {
                            EffectMap[chr, line] = 6;
                        }
                    }
                }

                if (gotPublicPool)
                {
                    for (var y = 0; y < StaticModel.MapSizeY; y++)
                    {
                        for (var x = 0; x < StaticModel.MapSizeX; x++)
                        {
                            if (StaticModel.mRoomModelfx[x, y] != 0)
                            {
                                EffectMap[x, y] = StaticModel.mRoomModelfx[x, y];
                            }
                        }
                    }
                }
                /** COMENTADO YA QUE SALAS PUBLICAS NUEVA CRYPTO NO NECESARIO
                if (!string.IsNullOrEmpty(StaticModel.StaticFurniMap)) 
                {
                     * foreach (PublicRoomSquare square in StaticModel.Furnis)
                    {
                        if (square.Content.Contains("chair") || square.Content.Contains("sofa"))
                        {
                            mGameMap[square.X, square.Y] = 1;
                        } else {
                            mGameMap[square.X, square.Y] = 0;
                        }
                    }
                }*/
            }
            else
            {
                //mGameMap
                //mUserItemEffect
                EffectMap = new byte[Model.MapSizeX, Model.MapSizeY];
                GameMap = new byte[Model.MapSizeX, Model.MapSizeY];
                mItemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];

                //if (modelRemap)
                //    Model.Generate(); //Clears model
                for (var line = 0; line < Model.MapSizeY; line++)
                {
                    for (var chr = 0; chr < Model.MapSizeX; chr++)
                    {
                        GameMap[chr, line] = 0;
                        EffectMap[chr, line] = 0;
                        if (chr == Model.DoorX && line == Model.DoorY)
                        {
                            GameMap[chr, line] = 3;
                        }
                        else if (Model.SqState[chr, line] == SquareState.OPEN)
                        {
                            GameMap[chr, line] = 1;
                        }
                        else if (Model.SqState[chr, line] == SquareState.SEAT)
                        {
                            GameMap[chr, line] = 2;
                        }
                        else if (Model.SqState[chr, line] == SquareState.POOL)
                        {
                            EffectMap[chr, line] = 6;
                        }
                    }
                }

                if (gotPublicPool)
                {
                    for (var y = 0; y < StaticModel.MapSizeY; y++)
                    {
                        for (var x = 0; x < StaticModel.MapSizeX; x++)
                        {
                            if (StaticModel.mRoomModelfx[x, y] != 0)
                            {
                                EffectMap[x, y] = StaticModel.mRoomModelfx[x, y];
                            }
                        }
                    }
                }
            }

            var tmpItems = _room.GetRoomItemHandler().GetFloor.ToArray();
            foreach (var Item in tmpItems.ToList())
            {
                if (Item == null)
                {
                    continue;
                }

                if (!AddItemToMap(Item))
                {
                }
            }

            Array.Clear(tmpItems, 0, tmpItems.Length);
            tmpItems = null;
            if (_room.RoomBlockingEnabled == 0)
            {
                foreach (var user in _room.GetRoomUserManager().GetUserList().ToList())
                {
                    if (user == null)
                    {
                        continue;
                    }

                    user.SqState = GameMap[user.X, user.Y];
                    GameMap[user.X, user.Y] = 0;
                }
            }

            try
            {
                GameMap[Model.DoorX, Model.DoorY] = 3;
            }
            catch
            {
            }
        }

        private bool ConstructMapForItem(Item Item, Point Coord)
        {
            try
            {
                if (Coord.X > Model.MapSizeX - 1)
                {
                    Model.AddX();
                    GenerateMaps();
                    return false;
                }

                if (Coord.Y > Model.MapSizeY - 1)
                {
                    Model.AddY();
                    GenerateMaps();
                    return false;
                }

                if (Model.SqState[Coord.X, Coord.Y] == SquareState.BLOCKED)
                {
                    Model.OpenSquare(Coord.X, Coord.Y, Item.GetZ);
                }
                if (mItemHeightMap[Coord.X, Coord.Y] <= Item.TotalHeight)
                {
                    mItemHeightMap[Coord.X, Coord.Y] = Item.TotalHeight - Model.SqFloorHeight[Item.GetX, Item.GetY];
                    EffectMap[Coord.X, Coord.Y] = 0;
                    switch (Item.GetBaseItem().InteractionType)
                    {
                        case InteractionType.Pool:
                            EffectMap[Coord.X, Coord.Y] = 1;
                            break;
                        case InteractionType.NormalSkates:
                            EffectMap[Coord.X, Coord.Y] = 2;
                            break;
                        case InteractionType.IceSkates:
                            EffectMap[Coord.X, Coord.Y] = 3;
                            break;
                        case InteractionType.Lowpool:
                            EffectMap[Coord.X, Coord.Y] = 4;
                            break;
                        case InteractionType.Haloweenpool:
                            EffectMap[Coord.X, Coord.Y] = 5;
                            break;
                    }

                    //SwimHalloween
                    if (Item.GetBaseItem().Walkable) // If this item is walkable and on the floor, allow users to walk here.
                    {
                        if (GameMap[Coord.X, Coord.Y] != 3)
                        {
                            GameMap[Coord.X, Coord.Y] = 1;
                        }
                    }
                    else if (Item.GetZ <= Model.SqFloorHeight[Item.GetX, Item.GetY] + 0.1 &&
                             Item.GetBaseItem().InteractionType == InteractionType.Gate &&
                             Item.ExtraData == "1") // If this item is a gate, open, and on the floor, allow users to walk here.
                    {
                        if (GameMap[Coord.X, Coord.Y] != 3)
                        {
                            GameMap[Coord.X, Coord.Y] = 1;
                        }
                    }
                    else if (Item.GetBaseItem().IsSeat ||
                             Item.GetBaseItem().InteractionType == InteractionType.Bed ||
                             Item.GetBaseItem().InteractionType == InteractionType.TentSmall)
                    {
                        GameMap[Coord.X, Coord.Y] = 3;
                    }
                    else // Finally, if it's none of those, block the square.
                    {
                        if (GameMap[Coord.X, Coord.Y] != 3)
                        {
                            GameMap[Coord.X, Coord.Y] = 0;
                        }
                    }
                }

                // Set bad maps
                if (Item.GetBaseItem().InteractionType == InteractionType.Bed ||
                    Item.GetBaseItem().InteractionType == InteractionType.TentSmall)
                {
                    GameMap[Coord.X, Coord.Y] = 3;
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }

            return true;
        }

        public void AddCoordinatedItem(Item item, Point coord)
        {
            var Items = new List<int>(); //mCoordinatedItems[CoordForItem];
            if (!mCoordinatedItems.TryGetValue(coord, out Items))
            {
                Items = new List<int>();
                if (!Items.Contains(item.Id))
                {
                    Items.Add(item.Id);
                }
                if (!mCoordinatedItems.ContainsKey(coord))
                {
                    mCoordinatedItems.TryAdd(coord, Items);
                }
            }
            else
            {
                if (!Items.Contains(item.Id))
                {
                    Items.Add(item.Id);
                    mCoordinatedItems[coord] = Items;
                }
            }
        }

        public List<Item> GetCoordinatedItems(Point coord)
        {
            var point = new Point(coord.X, coord.Y);
            var Items = new List<Item>();
            if (mCoordinatedItems.ContainsKey(point))
            {
                var Ids = mCoordinatedItems[point];
                Items = GetItemsFromIds(Ids);
                return Items;
            }

            return new List<Item>();
        }

        public bool RemoveCoordinatedItem(Item item, Point coord)
        {
            var point = new Point(coord.X, coord.Y);
            if (mCoordinatedItems != null && mCoordinatedItems.ContainsKey(point))
            {
                mCoordinatedItems[point].RemoveAll(x => x == item.Id);
                return true;
            }

            return false;
        }

        private void AddSpecialItems(Item item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.FootballGate:

                    //IsTrans = true;
                    _room.GetSoccer().RegisterGate(item);
                    var splittedExtraData = item.ExtraData.Split(':');
                    if (string.IsNullOrEmpty(item.ExtraData) || splittedExtraData.Length <= 1)
                    {
                        item.Gender = "M";
                        switch (item.Team)
                        {
                            case TEAM.YELLOW:
                                item.Figure = "lg-275-93.hr-115-61.hd-207-14.ch-265-93.sh-305-62";
                                break;
                            case TEAM.RED:
                                item.Figure = "lg-275-96.hr-115-61.hd-180-3.ch-265-96.sh-305-62";
                                break;
                            case TEAM.GREEN:
                                item.Figure = "lg-275-102.hr-115-61.hd-180-3.ch-265-102.sh-305-62";
                                break;
                            case TEAM.BLUE:
                                item.Figure = "lg-275-108.hr-115-61.hd-180-3.ch-265-108.sh-305-62";
                                break;
                        }
                    }
                    else
                    {
                        item.Gender = splittedExtraData[0];
                        item.Figure = splittedExtraData[1];
                    }

                    break;
                case InteractionType.Banzaifloor:
                {
                    _room.GetBanzai().AddTile(item, item.Id);
                    break;
                }
                case InteractionType.Banzaipyramid:
                {
                    _room.GetGameItemHandler().AddPyramid(item, item.Id);
                    break;
                }
                case InteractionType.Banzaitele:
                {
                    _room.GetGameItemHandler().AddTeleport(item, item.Id);
                    item.ExtraData = "";
                    break;
                }
                case InteractionType.Banzaipuck:
                {
                    _room.GetBanzai().AddPuck(item);
                    break;
                }
                case InteractionType.Football:
                {
                    _room.GetSoccer().AddBall(item);
                    break;
                }
                case InteractionType.FreezeTileBlock:
                {
                    _room.GetFreeze().AddFreezeBlock(item);
                    break;
                }
                case InteractionType.FreezeTile:
                {
                    _room.GetFreeze().AddFreezeTile(item);
                    break;
                }
                case InteractionType.Freezeexit:
                {
                    _room.GetFreeze().AddExitTile(item);
                    break;
                }
            }
        }

        private void RemoveSpecialItem(Item item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.FootballGate:
                    _room.GetSoccer().UnRegisterGate(item);
                    break;
                case InteractionType.Banzaifloor:
                    _room.GetBanzai().RemoveTile(item.Id);
                    break;
                case InteractionType.Banzaipuck:
                    _room.GetBanzai().RemovePuck(item.Id);
                    break;
                case InteractionType.Banzaipyramid:
                    _room.GetGameItemHandler().RemovePyramid(item.Id);
                    break;
                case InteractionType.Banzaitele:
                    _room.GetGameItemHandler().RemoveTeleport(item.Id);
                    break;
                case InteractionType.Football:
                    _room.GetSoccer().RemoveBall(item.Id);
                    break;
                case InteractionType.FreezeTile:
                    _room.GetFreeze().RemoveFreezeTile(item.Id);
                    break;
                case InteractionType.FreezeTileBlock:
                    _room.GetFreeze().RemoveFreezeBlock(item.Id);
                    break;
                case InteractionType.Freezeexit:
                    _room.GetFreeze().RemoveExitTile(item.Id);
                    break;
            }
        }

        public bool RemoveFromMap(Item item, bool handleGameItem)
        {
            if (handleGameItem)
            {
                RemoveSpecialItem(item);
            }
            if (_room.GotSoccer())
            {
                _room.GetSoccer().OnGateRemove(item);
            }
            var isRemoved = false;
            foreach (var coord in item.GetCoords.ToList())
            {
                if (RemoveCoordinatedItem(item, coord))
                {
                    isRemoved = true;
                }
            }

            var items = new ConcurrentDictionary<Point, List<Item>>();
            foreach (var Tile in item.GetCoords.ToList())
            {
                var point = new Point(Tile.X, Tile.Y);
                if (mCoordinatedItems.ContainsKey(point))
                {
                    var Ids = mCoordinatedItems[point];
                    var __items = GetItemsFromIds(Ids);
                    if (!items.ContainsKey(Tile))
                    {
                        items.TryAdd(Tile, __items);
                    }
                }
                SetDefaultValue(Tile.X, Tile.Y);
            }
            foreach (var Coord in items.Keys.ToList())
            {
                if (!items.ContainsKey(Coord))
                {
                    continue;
                }

                var SubItems = items[Coord];
                foreach (var Item in SubItems.ToList())
                {
                    ConstructMapForItem(Item, Coord);
                }
            }

            items.Clear();
            items = null;
            return isRemoved;
        }

        public bool RemoveFromMap(Item item) => RemoveFromMap(item, true);

        public bool AddItemToMap(Item Item, bool handleGameItem, bool NewItem = true)
        {
            if (handleGameItem)
            {
                AddSpecialItems(Item);
                switch (Item.GetBaseItem().InteractionType)
                {
                    case InteractionType.FootballGoalRed:
                    case InteractionType.Footballcounterred:
                    case InteractionType.Banzaiscorered:
                    case InteractionType.Banzaigatered:
                    case InteractionType.Freezeredcounter:
                    case InteractionType.FreezeRedGate:
                    {
                        if (!_room.GetRoomItemHandler().GetFloor.Contains(Item))
                        {
                            _room.GetGameManager().AddFurnitureToTeam(Item, TEAM.RED);
                        }
                        break;
                    }
                    case InteractionType.FootballGoalGreen:
                    case InteractionType.Footballcountergreen:
                    case InteractionType.Banzaiscoregreen:
                    case InteractionType.Banzaigategreen:
                    case InteractionType.Freezegreencounter:
                    case InteractionType.FreezeGreenGate:
                    {
                        if (!_room.GetRoomItemHandler().GetFloor.Contains(Item))
                        {
                            _room.GetGameManager().AddFurnitureToTeam(Item, TEAM.GREEN);
                        }
                        break;
                    }
                    case InteractionType.FootballGoalBlue:
                    case InteractionType.Footballcounterblue:
                    case InteractionType.Banzaiscoreblue:
                    case InteractionType.Banzaigateblue:
                    case InteractionType.Freezebluecounter:
                    case InteractionType.FreezeBlueGate:
                    {
                        if (!_room.GetRoomItemHandler().GetFloor.Contains(Item))
                        {
                            _room.GetGameManager().AddFurnitureToTeam(Item, TEAM.BLUE);
                        }
                        break;
                    }
                    case InteractionType.FootballGoalYellow:
                    case InteractionType.Footballcounteryellow:
                    case InteractionType.Banzaiscoreyellow:
                    case InteractionType.Banzaigateyellow:
                    case InteractionType.Freezeyellowcounter:
                    case InteractionType.FreezeYellowGate:
                    {
                        if (!_room.GetRoomItemHandler().GetFloor.Contains(Item))
                        {
                            _room.GetGameManager().AddFurnitureToTeam(Item, TEAM.YELLOW);
                        }
                        break;
                    }
                    case InteractionType.Freezeexit:
                    {
                        _room.GetFreeze().AddExitTile(Item);
                        break;
                    }
                    case InteractionType.Roller:
                    {
                        if (!_room.GetRoomItemHandler().GetRollers().Contains(Item))
                        {
                            _room.GetRoomItemHandler().TryAddRoller(Item.Id, Item);
                        }
                        break;
                    }
                }
            }

            if (Item.GetBaseItem().Type != 's')
            {
                return true;
            }

            foreach (var coord in Item.GetCoords.ToList())
            {
                AddCoordinatedItem(Item, new Point(coord.X, coord.Y));
            }

            if (Item.GetX > Model.MapSizeX - 1)
            {
                Model.AddX();
                GenerateMaps();
                return false;
            }

            if (Item.GetY > Model.MapSizeY - 1)
            {
                Model.AddY();
                GenerateMaps();
                return false;
            }

            var Return = true;
            foreach (var coord in Item.GetCoords)
            {
                if (!ConstructMapForItem(Item, coord))
                {
                    Return = false;
                }
                else
                {
                    Return = true;
                }
            }

            return Return;
        }

        public bool CanWalk(int X, int Y, bool Override)
        {
            if (Override)
            {
                return true;
            }
            if (_room.GetRoomUserManager().GetUserForSquare(X, Y) != null && _room.RoomBlockingEnabled == 0)
            {
                return false;
            }

            return true;
        }

        public bool AddItemToMap(Item Item, bool NewItem = true) => AddItemToMap(Item, true, NewItem);

        public bool ItemCanMove(Item Item, Point MoveTo)
        {
            var Points = GetAffectedTiles(Item.GetBaseItem().Length, Item.GetBaseItem().Width, MoveTo.X, MoveTo.Y, Item.Rotation)
                .Values.ToList();
            if (Points == null || Points.Count == 0)
            {
                return true;
            }

            foreach (var Coord in Points)
            {
                if (Coord.X >= Model.MapSizeX || Coord.Y >= Model.MapSizeY)
                {
                    return false;
                }
                if (!SquareIsOpen(Coord.X, Coord.Y, false))
                {
                    return false;
                }
            }

            return true;
        }

        public byte GetFloorStatus(Point coord)
        {
            if (coord.X > GameMap.GetUpperBound(0) || coord.Y > GameMap.GetUpperBound(1))
            {
                return 1;
            }

            return GameMap[coord.X, coord.Y];
        }

        public void SetFloorStatus(int X, int Y, byte Status)
        {
            GameMap[X, Y] = Status;
        }

        public double GetHeightForSquareFromData(Point coord)
        {
            if (coord.X > Model.SqFloorHeight.GetUpperBound(0) || coord.Y > Model.SqFloorHeight.GetUpperBound(1))
            {
                return 1;
            }

            return Model.SqFloorHeight[coord.X, coord.Y];
        }

        public bool CanRollItemHere(int x, int y)
        {
            if (!ValidTile(x, y))
            {
                return false;
            }
            if (Model.SqState[x, y] == SquareState.BLOCKED)
            {
                return false;
            }

            return true;
        }

        public bool SquareIsOpen(int x, int y, bool pOverride)
        {
            if (Model.MapSizeX - 1 < x || Model.MapSizeY - 1 < y)
            {
                return false;
            }

            return CanWalk(GameMap[x, y], pOverride);
        }

        public bool GetHighestItemForSquare(Point Square, out Item Item)
        {
            var Items = GetAllRoomItemForSquare(Square.X, Square.Y);
            Item = null;
            double HighestZ = -1;
            if (Items != null && Items.Count > 0)
            {
                foreach (var uItem in Items.ToList())
                {
                    if (uItem == null)
                    {
                        continue;
                    }

                    if (uItem.TotalHeight > HighestZ)
                    {
                        HighestZ = uItem.TotalHeight;
                        Item = uItem;
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public double GetHeightForSquare(Point Coord)
        {
            Item rItem;
            if (GetHighestItemForSquare(Coord, out rItem))
            {
                if (rItem != null)
                {
                    return rItem.TotalHeight;
                }
            }

            return 0.0;
        }

        public Point GetChaseMovement(Item Item)
        {
            var Distance = 99;
            var Coord = new Point(0, 0);
            var iX = Item.GetX;
            var iY = Item.GetY;
            var X = false;
            foreach (var User in _room.GetRoomUserManager().GetRoomUsers())
            {
                if (User.X == Item.GetX || Item.GetY == User.Y)
                {
                    if (User.X == Item.GetX)
                    {
                        var Difference = Math.Abs(User.Y - Item.GetY);
                        if (Difference < Distance)
                        {
                            Distance = Difference;
                            Coord = User.Coordinate;
                            X = false;
                        }
                    }
                    else if (User.Y == Item.GetY)
                    {
                        var Difference = Math.Abs(User.X - Item.GetX);
                        if (Difference < Distance)
                        {
                            Distance = Difference;
                            Coord = User.Coordinate;
                            X = true;
                        }
                    }
                }
            }

            if (Distance > 5)
            {
                return Item.GetSides().OrderBy(x => Guid.NewGuid()).FirstOrDefault();
            }

            if (X && Distance < 99)
            {
                if (iX > Coord.X)
                {
                    iX--;
                    return new Point(iX, iY);
                }

                iX++;
                return new Point(iX, iY);
            }

            if (!X && Distance < 99)
            {
                if (iY > Coord.Y)
                {
                    iY--;
                    return new Point(iX, iY);
                }

                iY++;
                return new Point(iX, iY);
            }

            return Item.Coordinate;
        }

        /*internal bool IsValidMovement(int CoordX, int CoordY)
        {
            if (CoordX < 0 || CoordY < 0 || CoordX >= Model.MapSizeX || CoordY >= Model.MapSizeY)
                return false;

            if (SquareHasUsers(CoordX, CoordY))
                return false;

            if (GetCoordinatedItems(new Point(CoordX, CoordY)).Count > 0 && !SquareIsOpen(CoordX, CoordY, false))
                return false;

            return Model.SqState[CoordX, CoordY] == SquareState.OPEN;
        }*/

        public bool IsValidStep2(RoomUser User, Vector2D From, Vector2D To, bool EndOfPath, bool Override)
        {
            if (User == null)
            {
                return false;
            }
            if (!ValidTile(To.X, To.Y))
            {
                return false;
            }
            if (Override)
            {
                return true;
            }
            /*
             * 0 = blocked
             * 1 = open
             * 2 = last step
             * 3 = door
             * */
            var Items = _room.GetGameMap().GetAllRoomItemForSquare(To.X, To.Y);
            if (Items.Count > 0)
            {
                var HasGroupGate = Items.Count(x => x.GetBaseItem().InteractionType == InteractionType.GuildGate) > 0;
                if (HasGroupGate)
                {
                    var I = Items.FirstOrDefault(x => x.GetBaseItem().InteractionType == InteractionType.GuildGate);
                    if (I != null)
                    {
                        Group Group = null;
                        if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(I.GroupId, out Group))
                        {
                            return false;
                        }
                        if (User.GetClient() == null || User.GetClient().GetHabbo() == null)
                        {
                            return false;
                        }

                        if (Group.IsMember(User.GetClient().GetHabbo().Id))
                        {
                            I.InteractingUser = User.GetClient().GetHabbo().Id;
                            I.ExtraData = "1";
                            I.UpdateState(false, true);
                            I.RequestUpdate(4, true);
                            return true;
                        }

                        if (User.Path.Count > 0)
                        {
                            User.Path.Clear();
                        }
                        User.PathRecalcNeeded = false;
                        return false;
                    }
                }
            }

            var Chair = false;
            double HighestZ = -1;
            foreach (var Item in Items.ToList())
            {
                if (Item == null)
                {
                    continue;
                }

                if (Item.GetZ < HighestZ)
                {
                    Chair = false;
                    continue;
                }

                HighestZ = Item.GetZ;
                if (Item.GetBaseItem().IsSeat)
                {
                    Chair = true;
                }
            }

            if (GameMap[To.X, To.Y] == 3 && !EndOfPath && !Chair || GameMap[To.X, To.Y] == 0 ||
                GameMap[To.X, To.Y] == 2 && !EndOfPath)
            {
                if (User.Path.Count > 0)
                {
                    User.Path.Clear();
                }
                User.PathRecalcNeeded = true;
            }
            var HeightDiff = SqAbsoluteHeight(To.X, To.Y) - SqAbsoluteHeight(From.X, From.Y);
            if (HeightDiff > 1.5 && !User.RidingHorse)
            {
                return false;
            }
            
            var userx = _room.GetRoomUserManager().GetUserForSquare(To.X, To.Y);

            if (userx == null)
            {
                return true;
            }

            return userx.IsWalking || !EndOfPath;
        }

        internal bool IsValidStep(Vector2D @from, Vector2D To, bool endOfPath, bool Override, bool roller = false)
        {
            if (!ValidTile(To.X, To.Y))
            {
                return false;
            }
            if (Override)
            {
                return true;
            }
            
            if (_room.RoomBlockingEnabled == 0 && SquareHasUsers(To.X, To.Y))
            {
                return false;
            }

            var items = _room.GetGameMap().GetAllRoomItemForSquare(To.X, To.Y);

            if (items.Count > 0)
            {
                var hasGroupGate = items.Count(x => x != null && x.GetBaseItem().InteractionType == InteractionType.GuildGate) > 0;

                if (hasGroupGate)
                {
                    return true;
                }
            }

            if (GameMap[To.X, To.Y] == 3 && !endOfPath || GameMap[To.X, To.Y] == 0 || GameMap[To.X, To.Y] == 2 && !endOfPath)
            {
                return false;
            }

            if (roller)
            {
                return true;
            }

            var heightDiff = SqAbsoluteHeight(To.X, To.Y) - SqAbsoluteHeight(@from.X, @from.Y);
            
            return !(heightDiff > 1.5);
        }

        private static bool CanWalk(byte pState, bool pOverride)
        {
            if (pOverride)
            {
                return true;
            }

            if (pState == 3)
            {
                return true;
            }
            
            return pState == 1;
        }

        internal bool ItemCanBePlacedHere(int x, int y)
        {
            if (Model.MapSizeX - 1 < x || Model.MapSizeY - 1 < y || x == Model.DoorX && y == Model.DoorY)
            {
                return false;
            }

            return GameMap[x, y] == 1;
        }

        internal double SqAbsoluteHeight(int X, int Y)
        {
            var Points = new Point(X, Y);

            if (!mCoordinatedItems.TryGetValue(Points, out var ids))
            {
                return Model.SqFloorHeight[X, Y];
            }

            var items = GetItemsFromIds(ids);
            
            return SqAbsoluteHeight(X, Y, items);
        }

        internal double SqAbsoluteHeight(int X, int Y, List<Item> itemsOnSquare)
        {
            try
            {
                var deduct = false;
                double highestStack = 0;
                var deductable = 0.0;
                
                if (itemsOnSquare != null && itemsOnSquare.Count > 0)
                {
                    foreach (var item in itemsOnSquare.ToList())
                    {
                        if (!(item?.TotalHeight > highestStack))
                        {
                            continue;
                        }

                        if (item.GetBaseItem().IsSeat ||
                            item.GetBaseItem().InteractionType == InteractionType.Bed ||
                            item.GetBaseItem().InteractionType == InteractionType.TentSmall)
                        {
                            deduct = true;
                            deductable = item.GetBaseItem().Height;
                        }
                        else
                        {
                            deduct = false;
                        }
                        
                        highestStack = item.TotalHeight;
                    }
                }

                double floorHeight = Model.SqFloorHeight[X, Y];
                var stackHeight = highestStack - Model.SqFloorHeight[X, Y];
                
                if (deduct)
                {
                    stackHeight -= deductable;
                }
                if (stackHeight < 0)
                {
                    stackHeight = 0;
                }
                return floorHeight + stackHeight;
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
                return 0;
            }
        }

        public bool ValidTile(int X, int Y)
        {
            if (X < 0 || Y < 0 || X >= Model.MapSizeX || Y >= Model.MapSizeY)
            {
                return false;
            }

            return true;
        }

        internal static Dictionary<int, ThreeDCoord> GetAffectedTiles(int Length, int Width, int PosX, int posY, int rotation)
        {
            var x = 0;
            var pointList = new Dictionary<int, ThreeDCoord>();
            
            if (Length > 1)
            {
                if (rotation == 0 || rotation == 4)
                {
                    for (var i = 1; i < Length; i++)
                    {
                        pointList.Add(x++, new ThreeDCoord(PosX, posY + i, i));
                        for (var j = 1; j < Width; j++)
                        {
                            pointList.Add(x++, new ThreeDCoord(PosX + j, posY + i, i < j ? j : i));
                        }
                    }
                }
                else if (rotation == 2 || rotation == 6)
                {
                    for (var i = 1; i < Length; i++)
                    {
                        pointList.Add(x++, new ThreeDCoord(PosX + i, posY, i));
                        for (var j = 1; j < Width; j++)
                        {
                            pointList.Add(x++, new ThreeDCoord(PosX + i, posY + j, i < j ? j : i));
                        }
                    }
                }
            }

            if (Width > 1)
            {
                if (rotation == 0 || rotation == 4)
                {
                    for (var i = 1; i < Width; i++)
                    {
                        pointList.Add(x++, new ThreeDCoord(PosX + i, posY, i));
                        for (var j = 1; j < Length; j++)
                        {
                            pointList.Add(x++, new ThreeDCoord(PosX + i, posY + j, i < j ? j : i));
                        }
                    }
                }
                else if (rotation == 2 || rotation == 6)
                {
                    for (var i = 1; i < Width; i++)
                    {
                        pointList.Add(x++, new ThreeDCoord(PosX, posY + i, i));
                        for (var j = 1; j < Length; j++)
                        {
                            pointList.Add(x++, new ThreeDCoord(PosX + j, posY + i, i < j ? j : i));
                        }
                    }
                }
            }

            return pointList;
        }

        public List<Item> GetItemsFromIds(List<int> input)
        {
            if (input == null || input.Count == 0)
            {
                return new List<Item>();
            }

            var items = new List<Item>();
            
            lock (input)
            {
                foreach (var Id in input.ToList())
                {
                    var Itm = _room.GetRoomItemHandler().GetItem(Id);
                    if (Itm != null && !items.Contains(Itm))
                    {
                        items.Add(Itm);
                    }
                }
            }

            return items.ToList();
        }

        public List<Item> GetRoomItemForSquare(int pX, int pY, double minZ)
        {
            var itemsToReturn = new List<Item>();
            var coord = new Point(pX, pY);
            if (mCoordinatedItems.ContainsKey(coord))
            {
                var itemsFromSquare = GetItemsFromIds(mCoordinatedItems[coord]);
                foreach (var item in itemsFromSquare)
                {
                    if (item.GetZ > minZ)
                    {
                        if (item.GetX == pX && item.GetY == pY)
                        {
                            itemsToReturn.Add(item);
                        }
                    }
                }
            }

            return itemsToReturn;
        }

        public List<Item> GetRoomItemForSquare(int pX, int pY)
        {
            var coord = new Point(pX, pY);

            //List<RoomItem> itemsFromSquare = new List<RoomItem>();
            var itemsToReturn = new List<Item>();
            if (mCoordinatedItems.ContainsKey(coord))
            {
                var itemsFromSquare = GetItemsFromIds(mCoordinatedItems[coord]);
                foreach (var item in itemsFromSquare)
                {
                    if (item.Coordinate.X == coord.X && item.Coordinate.Y == coord.Y)
                    {
                        itemsToReturn.Add(item);
                    }
                }
            }

            return itemsToReturn;
        }

        public List<Item> GetAllRoomItemForSquare(int pX, int pY)
        {
            var coord = new Point(pX, pY);
            var items = mCoordinatedItems.TryGetValue(coord, out var ids) ? GetItemsFromIds(ids) : new List<Item>();
            
            return items;
        }

        public bool SquareHasUsers(int X, int Y) => MapGotUser(new Point(X, Y));

        public static bool TilesTouching(int X1, int Y1, int X2, int Y2)
        {
            if (!(Math.Abs(X1 - X2) > 1 || Math.Abs(Y1 - Y2) > 1))
            {
                return true;
            }
            
            return X1 == X2 && Y1 == Y2;
        }

        public static int TileDistance(int X1, int Y1, int X2, int Y2) => Math.Abs(X1 - X2) + Math.Abs(Y1 - Y2);

        public void Dispose()
        {
            userMap.Clear();
            Model.Destroy();
            mCoordinatedItems.Clear();
            Array.Clear(GameMap, 0, GameMap.Length);
            Array.Clear(EffectMap, 0, EffectMap.Length);
            Array.Clear(mItemHeightMap, 0, mItemHeightMap.Length);
            userMap = null;
            GameMap = null;
            EffectMap = null;
            mItemHeightMap = null;
            mCoordinatedItems = null;
            Model = null;
            _room = null;
            StaticModel = null;
        }
    }
}