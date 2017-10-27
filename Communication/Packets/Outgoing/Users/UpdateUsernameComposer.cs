namespace Plus.Communication.Packets.Outgoing.Users
{
    internal class UpdateUsernameComposer : ServerPacket
    {
        public UpdateUsernameComposer(string user)
            : base(ServerPacketHeader.UpdateUsernameMessageComposer)
        {
            WriteInteger(0);
            WriteString(user);
            WriteInteger(0);
        }
    }
}