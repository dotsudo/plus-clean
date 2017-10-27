namespace Plus.Communication.Packets.Incoming.Groups
{
    using System;
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using Outgoing.Catalog;
    using Outgoing.Groups;
    using Outgoing.Inventory.Purse;
    using Outgoing.Moderation;
    using Outgoing.Rooms.Session;

    internal class PurchaseGroupEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var name = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(packet.PopString());
            var description = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(packet.PopString());
            var roomId = packet.PopInt();
            var colour1 = packet.PopInt();
            var colour2 = packet.PopInt();
            var unknown = packet.PopInt();

            var groupCost = Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("catalog.group.purchase.cost"));

            if (session.GetHabbo().Credits < groupCost)
            {
                session.SendPacket(new BroadcastMessageAlertComposer("A group costs " + groupCost + " credits! You only have " + session.GetHabbo().Credits + "!"));
                return;
            }

            session.GetHabbo().Credits -= groupCost;
            session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));

            var room = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomId);
            if (room == null || room.OwnerId != session.GetHabbo().Id || room.Group != null)
            {
                return;
            }

            var badge = string.Empty;

            for (var i = 0; i < 5; i++)
            {
                badge += BadgePartUtility.WorkBadgeParts(i == 0, packet.PopInt().ToString(), packet.PopInt().ToString(), packet.PopInt().ToString());
            }

            Group group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryCreateGroup(session.GetHabbo(), name, description, roomId, badge, colour1, colour2, out group))
            {
                session.SendNotification(
                    "An error occured whilst trying to create this group.\n\nTry again. If you get this message more than once, report it at the link below.\r\rhttp://boonboards.com");
                return;
            }

            session.SendPacket(new PurchaseOkComposer());

            room.Group = group;

            if (session.GetHabbo().CurrentRoomId != room.Id)
            {
                session.SendPacket(new RoomForwardComposer(room.Id));
            }

            session.SendPacket(new NewGroupInfoComposer(roomId, group.Id));
        }
    }
}