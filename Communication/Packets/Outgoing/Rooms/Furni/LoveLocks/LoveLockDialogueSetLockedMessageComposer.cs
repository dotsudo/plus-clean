namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.LoveLocks
{
    internal class LoveLockDialogueSetLockedMessageComposer : ServerPacket
    {
        public LoveLockDialogueSetLockedMessageComposer(int itemId)
            : base(ServerPacketHeader.LoveLockDialogueSetLockedMessageComposer)
        {
            WriteInteger(itemId);
        }
    }
}