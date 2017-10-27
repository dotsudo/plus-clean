namespace Plus.Communication.Packets.Outgoing.Inventory.Purse
{
    internal class ActivityPointsComposer : ServerPacket
    {
        public ActivityPointsComposer(int pixelsBalance, int seasionalCurrency, int gotwPoints)
            : base(ServerPacketHeader.ActivityPointsMessageComposer)
        {
            WriteInteger(11); //Count
            {
                WriteInteger(0); //Pixels
                WriteInteger(pixelsBalance);
                WriteInteger(1); //Snowflakes
                WriteInteger(16);
                WriteInteger(2); //Hearts
                WriteInteger(15);
                WriteInteger(3); //Gift points
                WriteInteger(14);
                WriteInteger(4); //Shells
                WriteInteger(13);
                WriteInteger(5); //Diamonds
                WriteInteger(seasionalCurrency);
                WriteInteger(101); //Snowflakes
                WriteInteger(10);
                WriteInteger(102);
                WriteInteger(0);
                WriteInteger(103); //Stars
                WriteInteger(gotwPoints);
                WriteInteger(104); //Clouds
                WriteInteger(0);
                WriteInteger(105); //Diamonds
                WriteInteger(0);
            }
        }
    }
}