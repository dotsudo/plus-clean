namespace Plus.Communication.Packets.Incoming.Users
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.GameClients;
    using Outgoing.Users;

    internal class CheckValidNameEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var inUse = false;
            var name = packet.PopString();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT COUNT(0) FROM `users` WHERE `username` = @name LIMIT 1");
                dbClient.AddParameter("name", name);
                inUse = dbClient.GetInteger() == 1;
            }

            var letters = name.ToLower().ToCharArray();
            var allowedCharacters = "abcdefghijklmnopqrstuvwxyz.,_-;:?!1234567890";

            foreach (var chr in letters)
            {
                if (!allowedCharacters.Contains(chr))
                {
                    session.SendPacket(new NameChangeUpdateComposer(name, 4));
                    return;
                }
            }

            if (PlusEnvironment.GetGame().GetChatManager().GetFilter().IsFiltered(name))
            {
                session.SendPacket(new NameChangeUpdateComposer(name, 4));
                return;
            }

            if (!session.GetHabbo().GetPermissions().HasRight("mod_tool") && name.ToLower().Contains("mod") || name.ToLower().Contains("adm") || name.ToLower().Contains("admin") ||
                name.ToLower().Contains("m0d"))
            {
                session.SendPacket(new NameChangeUpdateComposer(name, 4));
            }
            else if (!name.ToLower().Contains("mod") && (session.GetHabbo().Rank == 2 || session.GetHabbo().Rank == 3))
            {
                session.SendPacket(new NameChangeUpdateComposer(name, 4));
            }
            else if (name.Length > 15)
            {
                session.SendPacket(new NameChangeUpdateComposer(name, 3));
            }
            else if (name.Length < 3)
            {
                session.SendPacket(new NameChangeUpdateComposer(name, 2));
            }
            else if (inUse)
            {
                ICollection<string> suggestions = new List<string>();
                for (var i = 100; i < 103; i++)
                {
                    suggestions.Add(i.ToString());
                }

                session.SendPacket(new NameChangeUpdateComposer(name, 5, suggestions));
            }
            else
            {
                session.SendPacket(new NameChangeUpdateComposer(name, 0));
            }
        }
    }
}