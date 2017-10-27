namespace Plus.Communication.Packets.Outgoing.GameCenter
{
    using System.Collections.Generic;
    using HabboHotel.Games;

    internal class GameListComposer : ServerPacket
    {
        public GameListComposer(ICollection<GameData> games)
            : base(ServerPacketHeader.GameListMessageComposer)
        {
            WriteInteger(PlusEnvironment.GetGame().GetGameDataManager().GetCount()); //Game count
            foreach (var game in games)
            {
                WriteInteger(game.GameId);
                WriteString(game.GameName);
                WriteString(game.ColourOne);
                WriteString(game.ColourTwo);
                WriteString(game.ResourcePath);
                WriteString(game.StringThree);
            }
        }
    }
}