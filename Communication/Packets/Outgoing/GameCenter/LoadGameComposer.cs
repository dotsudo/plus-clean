namespace Plus.Communication.Packets.Outgoing.GameCenter
{
    using HabboHotel.Games;

    internal class LoadGameComposer : ServerPacket
    {
        public LoadGameComposer(GameData gameData, string ssoTicket)
            : base(ServerPacketHeader.LoadGameMessageComposer)
        {
            WriteInteger(gameData.GameId);
            WriteString("1365260055982");
            WriteString(gameData.ResourcePath + gameData.GameSWF);
            WriteString("best");
            WriteString("showAll");
            WriteInteger(60); //FPS?
            WriteInteger(10);
            WriteInteger(8);
            WriteInteger(6); //Asset count
            WriteString("assetUrl");
            WriteString(gameData.ResourcePath + gameData.GameAssets);
            WriteString("habboHost");
            WriteString("http://fuseus-private-httpd-fe-1");
            WriteString("accessToken");
            WriteString(ssoTicket);
            WriteString("gameServerHost");
            WriteString(gameData.GameServerHost);
            WriteString("gameServerPort");
            WriteString(gameData.GameServerPort);
            WriteString("socketPolicyPort");
            WriteString(gameData.GameServerHost);
        }
    }
}