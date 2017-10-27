namespace Plus.Communication.Packets.Outgoing.Handshake
{
    using HabboHotel.Users;

    public class UserObjectComposer : ServerPacket
    {
        public UserObjectComposer(Habbo habbo)
            : base(ServerPacketHeader.UserObjectMessageComposer)
        {
            WriteInteger(habbo.Id);
            WriteString(habbo.Username);
            WriteString(habbo.Look);
            WriteString(habbo.Gender.ToUpper());
            WriteString(habbo.Motto);
            WriteString("");
            WriteBoolean(false);
            WriteInteger(habbo.GetStats().Respect);
            WriteInteger(habbo.GetStats().DailyRespectPoints);
            WriteInteger(habbo.GetStats().DailyPetRespectPoints);
            WriteBoolean(false); // Friends stream active
            WriteString(habbo.LastOnline.ToString()); // last online?
            WriteBoolean(habbo.ChangingName); // Can change name
            WriteBoolean(false);
        }
    }
}