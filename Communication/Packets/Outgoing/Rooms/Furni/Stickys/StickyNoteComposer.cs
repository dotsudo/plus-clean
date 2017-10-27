namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.Stickys
{
    internal class StickyNoteComposer : ServerPacket
    {
        public StickyNoteComposer(string itemId, string extradata)
            : base(ServerPacketHeader.StickyNoteMessageComposer)
        {
            WriteString(itemId);
            WriteString(extradata);
        }
    }
}