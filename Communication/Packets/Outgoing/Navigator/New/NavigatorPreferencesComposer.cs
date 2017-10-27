namespace Plus.Communication.Packets.Outgoing.Navigator.New
{
    internal class NavigatorPreferencesComposer : ServerPacket
    {
        public NavigatorPreferencesComposer()
            : base(ServerPacketHeader.NavigatorPreferencesMessageComposer)
        {
            WriteInteger(68); //X
            WriteInteger(42); //Y
            WriteInteger(425); //Width
            WriteInteger(592); //Height
            WriteBoolean(false); //Show or hide saved searches.
            WriteInteger(0); //No idea?
        }
    }
}