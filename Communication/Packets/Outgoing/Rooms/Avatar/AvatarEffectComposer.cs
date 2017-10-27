namespace Plus.Communication.Packets.Outgoing.Rooms.Avatar
{
    internal class AvatarEffectComposer : ServerPacket
    {
        public AvatarEffectComposer(int playerId, int effectId)
            : base(ServerPacketHeader.AvatarEffectMessageComposer)
        {
            WriteInteger(playerId);
            WriteInteger(effectId);
            WriteInteger(0);
        }
    }
}