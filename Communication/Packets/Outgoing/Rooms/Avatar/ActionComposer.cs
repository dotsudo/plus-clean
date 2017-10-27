namespace Plus.Communication.Packets.Outgoing.Rooms.Avatar
{
    public class ActionComposer : ServerPacket
    {
        public ActionComposer(int virtualId, int action)
            : base(ServerPacketHeader.ActionMessageComposer)
        {
            WriteInteger(virtualId);
            WriteInteger(action);
        }
    }
}