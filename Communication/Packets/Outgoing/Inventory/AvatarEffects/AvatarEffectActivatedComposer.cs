namespace Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects
{
    using HabboHotel.Users.Effects;

    internal class AvatarEffectActivatedComposer : ServerPacket
    {
        public AvatarEffectActivatedComposer(AvatarEffect effect)
            : base(ServerPacketHeader.AvatarEffectActivatedMessageComposer)
        {
            WriteInteger(effect.SpriteId);
            WriteInteger((int) effect.Duration);
            WriteBoolean(false); //Permanent
        }
    }
}