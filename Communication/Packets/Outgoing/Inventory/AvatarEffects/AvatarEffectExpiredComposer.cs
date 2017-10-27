namespace Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects
{
    using HabboHotel.Users.Effects;

    internal class AvatarEffectExpiredComposer : ServerPacket
    {
        public AvatarEffectExpiredComposer(AvatarEffect effect)
            : base(ServerPacketHeader.AvatarEffectExpiredMessageComposer)
        {
            WriteInteger(effect.SpriteId);
        }
    }
}