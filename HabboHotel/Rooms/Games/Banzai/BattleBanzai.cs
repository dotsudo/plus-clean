﻿namespace Plus.HabboHotel.Rooms.Games.Banzai
{
    using System;
    using System.Collections.Concurrent;
    using System.Drawing;
    using System.Linq;
    using Communication.Packets.Outgoing.Rooms.Avatar;
    using Communication.Packets.Outgoing.Rooms.Engine;
    using GameClients;
    using Items;
    using Items.Wired;
    using PathFinding;
    using Teams;
    using Utilities.Enclosure;

    public class BattleBanzai
    {
        private ConcurrentDictionary<int, Item> _banzaiTiles;
        private ConcurrentDictionary<int, Item> _pucks;
        private Room _room;
        private GameField field;
        private byte[,] floorMap;
        private double timestarted;

        public BattleBanzai(Room room)
        {
            _room = room;
            isBanzaiActive = false;
            timestarted = 0;
            _pucks = new ConcurrentDictionary<int, Item>();
            _banzaiTiles = new ConcurrentDictionary<int, Item>();
        }

        public bool isBanzaiActive { get; private set; }

        public void AddTile(Item item, int itemId)
        {
            if (!_banzaiTiles.ContainsKey(itemId))
            {
                _banzaiTiles.TryAdd(itemId, item);
            }
        }

        public void RemoveTile(int itemId)
        {
            Item Item = null;
            _banzaiTiles.TryRemove(itemId, out Item);
        }

        public void AddPuck(Item item)
        {
            if (!_pucks.ContainsKey(item.Id))
            {
                _pucks.TryAdd(item.Id, item);
            }
        }

        public void RemovePuck(int itemID)
        {
            Item Item = null;
            _pucks.TryRemove(itemID, out Item);
        }

        public void OnUserWalk(RoomUser User)
        {
            if (User == null)
            {
                return;
            }

            foreach (var item in _pucks.Values.ToList())
            {
                var newX = 0;
                var newY = 0;
                var differenceX = User.X - item.GetX;
                var differenceY = User.Y - item.GetY;
                if (differenceX == 0 && differenceY == 0)
                {
                    if (User.RotBody == 4)
                    {
                        newX = User.X;
                        newY = User.Y + 2;
                    }
                    else if (User.RotBody == 6)
                    {
                        newX = User.X - 2;
                        newY = User.Y;
                    }
                    else if (User.RotBody == 0)
                    {
                        newX = User.X;
                        newY = User.Y - 2;
                    }
                    else if (User.RotBody == 2)
                    {
                        newX = User.X + 2;
                        newY = User.Y;
                    }
                    else if (User.RotBody == 1)
                    {
                        newX = User.X + 2;
                        newY = User.Y - 2;
                    }
                    else if (User.RotBody == 7)
                    {
                        newX = User.X - 2;
                        newY = User.Y - 2;
                    }
                    else if (User.RotBody == 3)
                    {
                        newX = User.X + 2;
                        newY = User.Y + 2;
                    }
                    else if (User.RotBody == 5)
                    {
                        newX = User.X - 2;
                        newY = User.Y + 2;
                    }
                    if (!_room.GetRoomItemHandler().CheckPosItem(User.GetClient(), item, newX, newY, item.Rotation, false, false))
                    {
                        if (User.RotBody == 0)
                        {
                            newX = User.X;
                            newY = User.Y + 1;
                        }
                        else if (User.RotBody == 2)
                        {
                            newX = User.X - 1;
                            newY = User.Y;
                        }
                        else if (User.RotBody == 4)
                        {
                            newX = User.X;
                            newY = User.Y - 1;
                        }
                        else if (User.RotBody == 6)
                        {
                            newX = User.X + 1;
                            newY = User.Y;
                        }
                        else if (User.RotBody == 5)
                        {
                            newX = User.X + 1;
                            newY = User.Y - 1;
                        }
                        else if (User.RotBody == 3)
                        {
                            newX = User.X - 1;
                            newY = User.Y - 1;
                        }
                        else if (User.RotBody == 7)
                        {
                            newX = User.X + 1;
                            newY = User.Y + 1;
                        }
                        else if (User.RotBody == 1)
                        {
                            newX = User.X - 1;
                            newY = User.Y + 1;
                        }
                    }
                }
                else if (differenceX <= 1 &&
                         differenceX >= -1 &&
                         differenceY <= 1 &&
                         differenceY >= -1 &&
                         VerifyPuck(User, item.Coordinate.X, item.Coordinate.Y)
                )
                {
                    newX = differenceX * -1;
                    newY = differenceY * -1;
                    newX = newX + item.GetX;
                    newY = newY + item.GetY;
                }
                if (item.GetRoom().GetGameMap().ValidTile(newX, newY))
                {
                    MovePuck(item, User.GetClient(), newX, newY, User.Team);
                }
            }

            if (isBanzaiActive)
            {
                HandleBanzaiTiles(User.Coordinate, User.Team, User);
            }
        }

        private static bool VerifyPuck(RoomUser user, int actualx, int actualy) =>
            Rotation.Calculate(user.X, user.Y, actualx, actualy) == user.RotBody;

        internal void BanzaiStart()
        {
            if (isBanzaiActive)
            {
                return;
            }

            floorMap = new byte[_room.GetGameMap().Model.MapSizeY, _room.GetGameMap().Model.MapSizeX];
            field = new GameField(floorMap, true);
            timestarted = PlusEnvironment.GetUnixTimestamp();
            _room.GetGameManager().LockGates();
            for (var i = 1; i < 5; i++)
            {
                _room.GetGameManager().Points[i] = 0;
            }
            foreach (var tile in _banzaiTiles.Values)
            {
                tile.ExtraData = "1";
                tile.Value = 0;
                tile.Team = TEAM.NONE;
                tile.UpdateState();
            }

            ResetTiles();
            isBanzaiActive = true;
            _room.GetWired().TriggerEvent(WiredBoxType.TriggerGameStarts, null);
            foreach (var user in _room.GetRoomUserManager().GetRoomUsers())
            {
                user.LockedTilesCount = 0;
            }
        }

        private void ResetTiles()
        {
            foreach (var item in _room.GetRoomItemHandler().GetFloor.ToList())
            {
                var type = item.GetBaseItem().InteractionType;
                switch (type)
                {
                    case InteractionType.Banzaiscoreblue:
                    case InteractionType.Banzaiscoregreen:
                    case InteractionType.Banzaiscorered:
                    case InteractionType.Banzaiscoreyellow:
                    {
                        item.ExtraData = "0";
                        item.UpdateState();
                        break;
                    }
                }
            }
        }

        internal void BanzaiEnd(bool userTriggered = false)
        {
            //TODO
            isBanzaiActive = false;
            _room.GetGameManager().StopGame();
            floorMap = null;
            if (!userTriggered)
            {
                _room.GetWired().TriggerEvent(WiredBoxType.TriggerGameEnds, null);
            }
            var winners = _room.GetGameManager().GetWinningTeam();
            _room.GetGameManager().UnlockGates();
            foreach (var tile in _banzaiTiles.Values)
            {
                if (tile.Team == winners)
                {
                    tile.InteractionCount = 0;
                    tile.InteractionCountHelper = 0;
                    tile.UpdateNeeded = true;
                }
                else if (tile.Team == TEAM.NONE)
                {
                    tile.ExtraData = "0";
                    tile.UpdateState();
                }
            }

            if (winners != TEAM.NONE)
            {
                var Winners = _room.GetRoomUserManager().GetRoomUsers();
                foreach (var User in Winners.ToList())
                {
                    if (User.Team != TEAM.NONE)
                    {
                        if (PlusEnvironment.GetUnixTimestamp() - timestarted > 5)
                        {
                            PlusEnvironment.GetGame()
                                .GetAchievementManager()
                                .ProgressAchievement(User.GetClient(), "ACH_BattleBallTilesLocked", User.LockedTilesCount);
                            PlusEnvironment.GetGame().GetAchievementManager()
                                .ProgressAchievement(User.GetClient(), "ACH_BattleBallPlayer", 1);
                        }
                    }
                    if (winners == TEAM.BLUE)
                    {
                        if (User.CurrentEffect == 35)
                        {
                            if (PlusEnvironment.GetUnixTimestamp() - timestarted > 5)
                            {
                                PlusEnvironment.GetGame().GetAchievementManager()
                                    .ProgressAchievement(User.GetClient(), "ACH_BattleBallWinner", 1);
                            }
                            _room.SendPacket(new ActionComposer(User.VirtualId, 1));
                        }
                    }
                    else if (winners == TEAM.RED)
                    {
                        if (User.CurrentEffect == 33)
                        {
                            if (PlusEnvironment.GetUnixTimestamp() - timestarted > 5)
                            {
                                PlusEnvironment.GetGame().GetAchievementManager()
                                    .ProgressAchievement(User.GetClient(), "ACH_BattleBallWinner", 1);
                            }
                            _room.SendPacket(new ActionComposer(User.VirtualId, 1));
                        }
                    }
                    else if (winners == TEAM.GREEN)
                    {
                        if (User.CurrentEffect == 34)
                        {
                            if (PlusEnvironment.GetUnixTimestamp() - timestarted > 5)
                            {
                                PlusEnvironment.GetGame().GetAchievementManager()
                                    .ProgressAchievement(User.GetClient(), "ACH_BattleBallWinner", 1);
                            }
                            _room.SendPacket(new ActionComposer(User.VirtualId, 1));
                        }
                    }
                    else if (winners == TEAM.YELLOW)
                    {
                        if (User.CurrentEffect == 36)
                        {
                            if (PlusEnvironment.GetUnixTimestamp() - timestarted > 5)
                            {
                                PlusEnvironment.GetGame().GetAchievementManager()
                                    .ProgressAchievement(User.GetClient(), "ACH_BattleBallWinner", 1);
                            }
                            _room.SendPacket(new ActionComposer(User.VirtualId, 1));
                        }
                    }
                }

                if (field != null)
                {
                    field.Dispose();
                }
            }
        }

        public void MovePuck(Item item, GameClient mover, int newX, int newY, TEAM team)
        {
            if (!_room.GetGameMap().ItemCanBePlacedHere(newX, newY))
            {
                return;
            }

            var oldRoomCoord = item.Coordinate;
            if (oldRoomCoord.X == newX && oldRoomCoord.Y == newY)
            {
                return;
            }

            item.ExtraData = Convert.ToInt32(team).ToString();
            item.UpdateNeeded = true;
            item.UpdateState();
            double NewZ = _room.GetGameMap().Model.SqFloorHeight[newX, newY];
            _room.SendPacket(new SlideObjectBundleComposer(item.GetX, item.GetY, item.GetZ, newX, newY, NewZ, 0, 0, item.Id));
            _room.GetRoomItemHandler().SetFloorItem(mover, item, newX, newY, item.Rotation, false, false, false);
            if (mover == null || mover.GetHabbo() == null)
            {
                return;
            }

            var user = mover.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(mover.GetHabbo().Id);
            if (isBanzaiActive)
            {
                HandleBanzaiTiles(new Point(newX, newY), team, user);
            }
        }

        private void SetTile(Item item, TEAM team, RoomUser user)
        {
            if (item.Team == team)
            {
                if (item.Value < 3)
                {
                    item.Value++;
                    if (item.Value == 3)
                    {
                        user.LockedTilesCount++;
                        _room.GetGameManager().AddPointToTeam(item.Team, 1);
                        field.UpdateLocation(item.GetX, item.GetY, (byte) team);
                        var gfield = field.DoUpdate();
                        TEAM t;
                        foreach (var gameField in gfield)
                        {
                            t = (TEAM) gameField.forValue;
                            foreach (var p in gameField.getPoints())
                            {
                                HandleMaxBanzaiTiles(new Point(p.X, p.Y), t);
                                floorMap[p.Y, p.X] = gameField.forValue;
                            }
                        }
                    }
                }
            }
            else
            {
                if (item.Value < 3)
                {
                    item.Team = team;
                    item.Value = 1;
                }
            }

            var newColor = item.Value + Convert.ToInt32(item.Team) * 3 - 1;
            item.ExtraData = newColor.ToString();
        }

        private void HandleBanzaiTiles(Point coord, TEAM team, RoomUser user)
        {
            if (team == TEAM.NONE)
            {
                return;
            }

            var i = 0;
            foreach (var _item in _banzaiTiles.Values.ToList())
            {
                if (_item == null)
                {
                    continue;
                }

                if (_item.GetBaseItem().InteractionType != InteractionType.Banzaifloor)
                {
                    user.Team = TEAM.NONE;
                    user.ApplyEffect(0);
                    continue;
                }

                if (_item.ExtraData.Equals("5") || _item.ExtraData.Equals("8") || _item.ExtraData.Equals("11") ||
                    _item.ExtraData.Equals("14"))
                {
                    i++;
                    continue;
                }

                if (_item.GetX != coord.X || _item.GetY != coord.Y)
                {
                    continue;
                }

                SetTile(_item, team, user);
                if (_item.ExtraData.Equals("5") || _item.ExtraData.Equals("8") || _item.ExtraData.Equals("11") ||
                    _item.ExtraData.Equals("14"))
                {
                    i++;
                }
                _item.UpdateState(false, true);
            }

            if (i == _banzaiTiles.Count)
            {
                BanzaiEnd();
            }
        }

        private void HandleMaxBanzaiTiles(Point coord, TEAM team)
        {
            if (team == TEAM.NONE)
            {
                return;
            }

            foreach (var item in _banzaiTiles.Values.ToList())
            {
                if (item?.GetBaseItem().InteractionType != InteractionType.Banzaifloor)
                {
                    continue;
                }
                if (item.GetX != coord.X || item.GetY != coord.Y)
                {
                    continue;
                }

                SetMaxForTile(item, team);
                _room.GetGameManager().AddPointToTeam(team, 1);
                item.UpdateState(false, true);
            }
        }

        private static void SetMaxForTile(Item item, TEAM team)
        {
            if (item.Value < 3)
            {
                item.Value = 3;
                item.Team = team;
            }

            var newColor = item.Value + Convert.ToInt32(item.Team) * 3 - 1;
            item.ExtraData = newColor.ToString();
        }

        internal void Dispose()
        {
            _banzaiTiles.Clear();
            _pucks.Clear();

            if (floorMap != null)
            {
                Array.Clear(floorMap, 0, floorMap.Length);
            }

            field?.Dispose();

            _room = null;
            _banzaiTiles = null;
            _pucks = null;
            floorMap = null;
            field = null;
        }
    }
}