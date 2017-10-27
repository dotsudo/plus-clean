namespace Plus.Communication.Packets.Outgoing.Catalog
{
    public class VoucherRedeemErrorComposer : ServerPacket
    {
        public VoucherRedeemErrorComposer(int type)
            : base(ServerPacketHeader.VoucherRedeemErrorMessageComposer)
        {
            WriteString(type.ToString());
        }
    }
}