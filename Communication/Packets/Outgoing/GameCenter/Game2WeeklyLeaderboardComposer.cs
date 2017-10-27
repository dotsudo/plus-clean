﻿namespace Plus.Communication.Packets.Outgoing.GameCenter
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.Games;
    using HabboHotel.Users;

    public class Game2WeeklyLeaderboardComposer : ServerPacket
    {
        public Game2WeeklyLeaderboardComposer(GameData gameData, ICollection<Habbo> habbos)
            : base(ServerPacketHeader.Game2WeeklyLeaderboardMessageComposer)
        {
            WriteInteger(2014);
            WriteInteger(41);
            WriteInteger(0);
            WriteInteger(1);
            WriteInteger(1581);

            //Used to generate the ranking numbers.
            var num = 0;

            WriteInteger(habbos.Count); //Count
            foreach (var habbo in habbos.ToList())
            {
                num++;
                WriteInteger(habbo.Id); //Id
                WriteInteger(habbo.FastfoodScore); //Score
                WriteInteger(num); //Rank
                WriteString(habbo.Username); //Username
                WriteString(habbo.Look); //Figure
                WriteString(habbo.Gender.ToLower()); //Gender .ToLower()
            }

            WriteInteger(0); //
            WriteInteger(gameData.GameId); //Game Id?
        }
    }
}