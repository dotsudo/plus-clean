﻿namespace Plus.HabboHotel.Rooms.AI.Types
{
    using System;
    using System.Drawing;
    using GameClients;

    public class GenericBot : BotAI
    {
        private readonly int VirtualId;
        private int ActionTimer;
        private int SpeechTimer;

        public GenericBot(int VirtualId) => this.VirtualId = VirtualId;

        public override void OnSelfEnterRoom()
        {
        }

        public override void OnSelfLeaveRoom(bool Kicked)
        {
        }

        public override void OnUserEnterRoom(RoomUser User)
        {
        }

        public override void OnUserLeaveRoom(GameClient Client)
        {
        }

        public override void OnUserSay(RoomUser User, string Message)
        {
        }

        public override void OnUserShout(RoomUser User, string Message)
        {
        }

        public override void OnTimerTick()
        {
            if (GetBotData() == null)
            {
                return;
            }

            if (SpeechTimer <= 0)
            {
                if (GetBotData().RandomSpeech.Count > 0)
                {
                    if (GetBotData().AutomaticChat == false)
                    {
                        return;
                    }

                    var Speech = GetBotData().GetRandomSpeech();
                    var String = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Speech.Message);
                    if (String.Contains("<img src") ||
                        String.Contains("<font ") ||
                        String.Contains("</font>") ||
                        String.Contains("</a>") ||
                        String.Contains("<i>"))
                    {
                        String = "I really shouldn't be using HTML within bot speeches.";
                    }
                    GetRoomUser().Chat(String, false, GetBotData().ChatBubble);
                }

                SpeechTimer = GetBotData().SpeakingInterval;
            }
            else
            {
                SpeechTimer--;
            }

            if (ActionTimer <= 0)
            {
                Point nextCoord;
                switch (GetBotData().WalkingMode.ToLower())
                {
                    default:
                    case "stand":

                        // (8) Why is my life so boring?
                        break;
                    case "freeroam":
                        if (GetBotData().ForcedMovement)
                        {
                            if (GetRoomUser().Coordinate == GetBotData().TargetCoordinate)
                            {
                                GetBotData().ForcedMovement = false;
                                GetBotData().TargetCoordinate = new Point();
                                GetRoomUser().MoveTo(GetBotData().TargetCoordinate.X, GetBotData().TargetCoordinate.Y);
                            }
                        }
                        else if (GetBotData().ForcedUserTargetMovement > 0)
                        {
                            var Target = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(GetBotData().ForcedUserTargetMovement);
                            if (Target == null)
                            {
                                GetBotData().ForcedUserTargetMovement = 0;
                                GetRoomUser().ClearMovement(true);
                            }
                            else
                            {
                                var Sq = new Point(Target.X, Target.Y);
                                if (Target.RotBody == 0)
                                {
                                    Sq.Y--;
                                }
                                else if (Target.RotBody == 2)
                                {
                                    Sq.X++;
                                }
                                else if (Target.RotBody == 4)
                                {
                                    Sq.Y++;
                                }
                                else if (Target.RotBody == 6)
                                {
                                    Sq.X--;
                                }
                                GetRoomUser().MoveTo(Sq);
                            }
                        }
                        else if (GetBotData().TargetUser == 0)
                        {
                            nextCoord = GetRoom().GetGameMap().GetRandomWalkableSquare();
                            GetRoomUser().MoveTo(nextCoord.X, nextCoord.Y);
                        }
                        break;
                    case "specified_range":
                        break;
                }

                ActionTimer = new Random((DateTime.Now.Millisecond + VirtualId) ^ 2).Next(5, 15);
            }
            else
            {
                ActionTimer--;
            }
        }
    }
}