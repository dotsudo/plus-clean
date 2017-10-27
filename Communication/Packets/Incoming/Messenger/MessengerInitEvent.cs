namespace Plus.Communication.Packets.Incoming.Messenger
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Users.Messenger;
    using MoreLinq;
    using Outgoing.Messenger;

    internal class MessengerInitEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            session.GetHabbo().GetMessenger().OnStatusChanged(false);

            ICollection<MessengerBuddy> friends = session.GetHabbo().GetMessenger().GetFriends().ToList().Where(buddy => buddy != null && !buddy.IsOnline).ToList();

            session.SendPacket(new MessengerInitComposer());

            var page = 0;
            if (!friends.Any())
            {
                session.SendPacket(new BuddyListComposer(friends, session.GetHabbo(), 1, 0));
            }
            else
            {
                var pages = (friends.Count - 1) / 500 + 1;
                foreach (var enumerable in friends.Batch(500))
                {
                    var batch = (ICollection<MessengerBuddy>) enumerable;
                    session.SendPacket(new BuddyListComposer(batch.ToList(), session.GetHabbo(), pages, page));

                    page++;
                }
            }

            session.GetHabbo().GetMessenger().ProcessOfflineMessages();
        }
    }
}