namespace Plus.Communication.Packets.Outgoing.Rooms.Avatar
{
    internal class AvatarAspectUpdateComposer : ServerPacket
    {
        public AvatarAspectUpdateComposer(string figure, string gender)
            : base(ServerPacketHeader.AvatarAspectUpdateMessageComposer)
        {
            WriteString(figure);
            WriteString(gender);
        }
    }
}