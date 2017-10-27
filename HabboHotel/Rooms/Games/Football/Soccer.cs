namespace Plus.HabboHotel.Rooms.Games.Football
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using Communication.Packets.Outgoing.Rooms.Engine;
    using Items;
    using Items.Wired;
    using PathFinding;
    using Teams;

    public class Soccer : IDisposable
    {
        private ConcurrentDictionary<int, Item> _balls;
        private Item[] _gates;
        private Room _room;

        internal Soccer(Room room)
        {
            _room = room;
            _gates = new Item[4];
            _balls = new ConcurrentDictionary<int, Item>();
            GameIsStarted = false;
        }

        internal bool GameIsStarted { get; private set; }

        public void Dispose()
        {
            Array.Clear(_gates, 0, _gates.Length);
            _gates = null;
            _room = null;
            _balls.Clear();
            _balls = null;
        }

        internal void StopGame(bool userTriggered = false)
        {
            GameIsStarted = false;
            if (!userTriggered)
            {
                _room.GetWired().TriggerEvent(WiredBoxType.TriggerGameEnds, null);
            }
        }

        internal void StartGame()
        {
            GameIsStarted = true;
        }

        internal void AddBall(Item item)
        {
            _balls.TryAdd(item.Id, item);
        }

        internal void RemoveBall(int itemID)
        {
            Item Item = null;
            _balls.TryRemove(itemID, out Item);
        }

        internal void OnUserWalk(RoomUser User)
        {
            if (User == null)
            {
                return;
            }

            foreach (var item in _balls.Values.ToList())
            {
                var NewX = 0;
                var NewY = 0;
                var differenceX = User.X - item.GetX;
                var differenceY = User.Y - item.GetY;
                if (differenceX == 0 && differenceY == 0)
                {
                    if (User.RotBody == 4)
                    {
                        NewX = User.X;
                        NewY = User.Y + 2;
                    }
                    else if (User.RotBody == 6)
                    {
                        NewX = User.X - 2;
                        NewY = User.Y;
                    }
                    else if (User.RotBody == 0)
                    {
                        NewX = User.X;
                        NewY = User.Y - 2;
                    }
                    else if (User.RotBody == 2)
                    {
                        NewX = User.X + 2;
                        NewY = User.Y;
                    }
                    else if (User.RotBody == 1)
                    {
                        NewX = User.X + 2;
                        NewY = User.Y - 2;
                    }
                    else if (User.RotBody == 7)
                    {
                        NewX = User.X - 2;
                        NewY = User.Y - 2;
                    }
                    else if (User.RotBody == 3)
                    {
                        NewX = User.X + 2;
                        NewY = User.Y + 2;
                    }
                    else if (User.RotBody == 5)
                    {
                        NewX = User.X - 2;
                        NewY = User.Y + 2;
                    }
                    if (!_room.GetRoomItemHandler().CheckPosItem(User.GetClient(), item, NewX, NewY, item.Rotation, false, false))
                    {
                        if (User.RotBody == 0)
                        {
                            NewX = User.X;
                            NewY = User.Y + 1;
                        }
                        else if (User.RotBody == 2)
                        {
                            NewX = User.X - 1;
                            NewY = User.Y;
                        }
                        else if (User.RotBody == 4)
                        {
                            NewX = User.X;
                            NewY = User.Y - 1;
                        }
                        else if (User.RotBody == 6)
                        {
                            NewX = User.X + 1;
                            NewY = User.Y;
                        }
                        else if (User.RotBody == 5)
                        {
                            NewX = User.X + 1;
                            NewY = User.Y - 1;
                        }
                        else if (User.RotBody == 3)
                        {
                            NewX = User.X - 1;
                            NewY = User.Y - 1;
                        }
                        else if (User.RotBody == 7)
                        {
                            NewX = User.X + 1;
                            NewY = User.Y + 1;
                        }
                        else if (User.RotBody == 1)
                        {
                            NewX = User.X - 1;
                            NewY = User.Y + 1;
                        }
                    }
                }
                else if (differenceX <= 1 &&
                         differenceX >= -1 &&
                         differenceY <= 1 &&
                         differenceY >= -1 &&
                         VerifyBall(User, item.Coordinate.X, item.Coordinate.Y)
                ) //VERYFIC BALL CHECAR SI ESTA EN DIRECCION ASIA LA PELOTA
                {
                    NewX = differenceX * -1;
                    NewY = differenceY * -1;
                    NewX = NewX + item.GetX;
                    NewY = NewY + item.GetY;
                }
                if (item.GetRoom().GetGameMap().ValidTile(NewX, NewY))
                {
                    MoveBall(item, NewX, NewY, User);
                }
            }
        }

        private bool VerifyBall(RoomUser user, int actualx, int actualy) =>
            Rotation.Calculate(user.X, user.Y, actualx, actualy) == user.RotBody;

        internal void RegisterGate(Item item)
        {
            if (_gates[0] == null)
            {
                item.Team = TEAM.BLUE;
                _gates[0] = item;
            }
            else if (_gates[1] == null)
            {
                item.Team = TEAM.RED;
                _gates[1] = item;
            }
            else if (_gates[2] == null)
            {
                item.Team = TEAM.GREEN;
                _gates[2] = item;
            }
            else if (_gates[3] == null)
            {
                item.Team = TEAM.YELLOW;
                _gates[3] = item;
            }
        }

        internal void UnRegisterGate(Item item)
        {
            switch (item.Team)
            {
                case TEAM.BLUE:
                {
                    _gates[0] = null;
                    break;
                }
                case TEAM.RED:
                {
                    _gates[1] = null;
                    break;
                }
                case TEAM.GREEN:
                {
                    _gates[2] = null;
                    break;
                }
                case TEAM.YELLOW:
                {
                    _gates[3] = null;
                    break;
                }
            }
        }

        internal void OnGateRemove(Item item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.FootballGoalRed:
                case InteractionType.Footballcounterred:
                {
                    _room.GetGameManager().RemoveFurnitureFromTeam(item, TEAM.RED);
                    break;
                }
                case InteractionType.FootballGoalGreen:
                case InteractionType.Footballcountergreen:
                {
                    _room.GetGameManager().RemoveFurnitureFromTeam(item, TEAM.GREEN);
                    break;
                }
                case InteractionType.FootballGoalBlue:
                case InteractionType.Footballcounterblue:
                {
                    _room.GetGameManager().RemoveFurnitureFromTeam(item, TEAM.BLUE);
                    break;
                }
                case InteractionType.FootballGoalYellow:
                case InteractionType.Footballcounteryellow:
                {
                    _room.GetGameManager().RemoveFurnitureFromTeam(item, TEAM.YELLOW);
                    break;
                }
            }
        }

        internal void MoveBall(Item item, int newX, int newY, RoomUser user)
        {
            if (item == null || user == null)
            {
                return;
            }
            if (!_room.GetGameMap().ItemCanBePlacedHere(newX, newY))
            {
                return;
            }

            var oldRoomCoord = item.Coordinate;
            if (oldRoomCoord.X == newX && oldRoomCoord.Y == newY)
            {
                return;
            }

            double NewZ = _room.GetGameMap().Model.SqFloorHeight[newX, newY];
            _room.SendPacket(
                new SlideObjectBundleComposer(item.Coordinate.X, item.Coordinate.Y, item.GetZ, newX, newY, NewZ, item.Id, item.Id,
                    item.Id));
            item.ExtraData = "11";
            item.UpdateNeeded = true;
            _room.GetRoomItemHandler().SetFloorItem(null, item, newX, newY, item.Rotation, false, false, false);
            _room.OnUserShoot(user, item);
        }
    }
}