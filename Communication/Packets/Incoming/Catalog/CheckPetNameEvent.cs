namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.GameClients;
    using Outgoing.Catalog;

    public class CheckPetNameEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var petName = packet.PopString();

            if (petName.Length < 2)
            {
                session.SendPacket(new CheckPetNameComposer(2, "2"));
                return;
            }

            if (petName.Length > 15)
            {
                session.SendPacket(new CheckPetNameComposer(1, "15"));
                return;
            }

            if (!PlusEnvironment.IsValidAlphaNumeric(petName))
            {
                session.SendPacket(new CheckPetNameComposer(3, ""));
                return;
            }

            if (PlusEnvironment.GetGame().GetChatManager().GetFilter().IsFiltered(petName))
            {
                session.SendPacket(new CheckPetNameComposer(4, ""));
                return;
            }

            session.SendPacket(new CheckPetNameComposer(0, ""));
        }
    }
}