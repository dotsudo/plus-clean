namespace Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects
{
    internal class AvatarEffectAddedComposer : ServerPacket
    {
        public AvatarEffectAddedComposer(int spriteId, int duration)
            : base(ServerPacketHeader.AvatarEffectAddedMessageComposer)
        {
            WriteInteger(spriteId);
            WriteInteger(0); //Types
            WriteInteger(duration);
            WriteBoolean(false); //Permanent
        }
    }
}