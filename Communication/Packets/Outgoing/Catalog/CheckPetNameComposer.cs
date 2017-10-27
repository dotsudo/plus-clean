namespace Plus.Communication.Packets.Outgoing.Catalog
{
    public class CheckPetNameComposer : ServerPacket
    {
        public CheckPetNameComposer(int error, string extraData)
            : base(ServerPacketHeader.CheckPetNameMessageComposer)
        {
            WriteInteger(error); //0 = nothing, 1 = too long, 2 = too short, 3 = invalid characters
            WriteString(extraData);
        }
    }
}